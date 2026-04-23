using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly GarageDbContext _context;

        public CheckInService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<CheckInResponseDTO> ProcessCheckInAsync(string licensePlate)
        {
            // Chuẩn hóa biển số: Viết hoa, xóa dấu cách, gạch ngang, dấu chấm
            string normalizedInput = licensePlate.ToUpper().Replace("-", "").Replace(".", "").Replace(" ", "");

            // Tìm xe và bao gồm thông tin khách hàng
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => (v.LicensePlate ?? "").Replace("-", "").Replace(".", "").Replace(" ", "").ToUpper() == normalizedInput);

            if (vehicle != null && vehicle.Customer != null)
            {
                // --- THÊM LOGIC LẤY LỊCH SỬ CHI TIẾT ---
                var history = await _context.RepairOrders
                    .Include(ro => ro.OrderServices).ThenInclude(os => os.Service)
                    .Include(ro => ro.OrderParts).ThenInclude(op => op.Part)
                    .Where(ro => ro.VehicleId == vehicle.Id && ro.Status == "Completed")
                    .OrderByDescending(ro => ro.CreatedAt)
                    .Select(ro => new {
                        OrderCode = ro.OrderCode,
                        CreatedAt = ro.CreatedAt,
                        CurrentOdometer = ro.CurrentOdometer, // Lấy số KM
                        FinalAmount = ro.FinalAmount,         // Lấy tổng tiền
                        Services = ro.OrderServices.Select(s => s.Service.ServiceName).ToList(),
                        Parts = ro.OrderParts.Select(p => p.Part.PartName).ToList() // Lấy phụ tùng
                    })
                    .Take(3).ToListAsync();

                // Tìm lịch hẹn trong ngày hôm nay (Dùng UtcNow để khớp với DB Render)
                var today = DateTime.UtcNow.Date;
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id
                                           && a.AppointmentDate.Date == today
                                           && a.Status != "Cancelled");

                return new CheckInResponseDTO
                {
                    IsExisting = true,
                    Message = $"Chào mừng {vehicle.Customer.FullName} quay trở lại!",
                    CustomerId = vehicle.Customer.Id,
                    CustomerName = vehicle.Customer.FullName ?? "",
                    PhoneNumber = vehicle.Customer.PhoneNumber ?? "",
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate ?? "",
                    Make = vehicle.Make ?? "",
                    Model = vehicle.Model ?? "",
                    ActiveAppointmentId = appointment?.Id, // Gán ID nếu tìm thấy lịch hẹn
                    MaintenanceHistory = history,
                };
            }

            return new CheckInResponseDTO
            {
                IsExisting = false,
                Message = "Xe mới lần đầu đến xưởng. Vui lòng tạo hồ sơ!",
                LicensePlate = licensePlate
            };
        }

        public async Task<CheckInResponseDTO> QuickOnboardAsync(QuickOnboardRequestDTO request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. TÌM KIẾM KHÁCH HÀNG ĐÃ TỒN TẠI
                    // Ưu tiên tìm theo Số điện thoại vì đây là định danh duy nhất của khách hàng
                    Customer? existingCustomer = null;
                    if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    {
                        existingCustomer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.PhoneNumber == request.PhoneNumber);
                    }

                    Guid finalCustomerId;
                    string finalCustomerName;
                    string finalPhone;

                    if (existingCustomer != null)
                    {
                        // TRƯỜNG HỢP 1: Khách đã có trong hệ thống (đã từng mang xe khác đến)
                        finalCustomerId = existingCustomer.Id;
                        finalCustomerName = existingCustomer.FullName;
                        finalPhone = existingCustomer.PhoneNumber ?? "";

                        // (Tùy chọn) Cập nhật thêm Email hoặc Địa chỉ nếu khách hàng bổ sung
                        if (string.IsNullOrEmpty(existingCustomer.Email) && !string.IsNullOrEmpty(request.Email))
                            existingCustomer.Email = request.Email;
                        if (string.IsNullOrEmpty(existingCustomer.Address) && !string.IsNullOrEmpty(request.Address))
                            existingCustomer.Address = request.Address;
                    }
                    else
                    {
                        // TRƯỜNG HỢP 2: Khách hàng hoàn toàn mới
                        var newCustomer = new Customer
                        {
                            FullName = request.CustomerName,
                            PhoneNumber = request.PhoneNumber,
                            Email = request.Email,
                            Address = request.Address,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Customers.Add(newCustomer);
                        await _context.SaveChangesAsync(); // Lưu để lấy ID

                        finalCustomerId = newCustomer.Id;
                        finalCustomerName = newCustomer.FullName;
                        finalPhone = newCustomer.PhoneNumber ?? "";
                    }

                    // 2. TẠO XE MỚI VÀ GÁN CHO KHÁCH HÀNG (Dù là khách cũ hay mới)
                    var vehicle = new Vehicle
                    {
                        LicensePlate = request.LicensePlate.ToUpper().Trim(),
                        CustomerId = finalCustomerId,
                        Make = "Chưa rõ", // SA hoặc thợ sẽ cập nhật sau trong lệnh sửa chữa
                        Model = "Chưa rõ"
                    };
                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();

                    // Xác nhận hoàn tất chuỗi thao tác
                    await transaction.CommitAsync();

                    return new CheckInResponseDTO
                    {
                        IsExisting = true,
                        Message = existingCustomer != null
                            ? "Đã liên kết xe mới vào hồ sơ khách hàng cũ thành công!"
                            : "Đã tạo mới hồ sơ khách hàng và xe thành công!",
                        CustomerId = finalCustomerId,
                        CustomerName = finalCustomerName,
                        PhoneNumber = finalPhone,
                        VehicleId = vehicle.Id,
                        LicensePlate = vehicle.LicensePlate,
                        Make = vehicle.Make,
                        Model = vehicle.Model
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Lỗi khi xử lý tiếp nhận nhanh: " + ex.Message);
                }
            });
        }
    }
}