using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly GarageDbContext _context;
        private readonly IEmailService _emailService;
        public AppointmentService(GarageDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<object> CreateAsync(AppointmentRequestDTO request)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null) return new { success = false, message = "Không tìm thấy khách hàng." };

            // 1. Dùng UtcNow để so sánh thay vì Now
            if (request.AppointmentDate < DateTime.UtcNow)
            {
                return new { success = false, message = "Ngày hẹn không được nhỏ hơn thời gian hiện tại." };
            }

            var appointment = new Appointment
            {
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                // 2. Ép kiểu thời gian khách gửi về chuẩn Utc để Postgres chấp nhận
                AppointmentDate = DateTime.SpecifyKind(request.AppointmentDate, DateTimeKind.Utc),
                ExpectedServices = request.ExpectedServices,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow // Luôn dùng UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(); // Sẽ không còn lỗi ở đây nữa

            return new { success = true, id = appointment.Id, message = "Đặt lịch thành công." };
        }

        public async Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle) // Lấy kèm thông tin Xe
                .AsQueryable();

            // Logic Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            // Logic Lọc theo khoảng thời gian hẹn (Rất cần cho Lễ tân xem lịch hôm nay)
            if (fromDate.HasValue) query = query.Where(a => a.AppointmentDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(a => a.AppointmentDate <= toDate.Value);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .OrderBy(a => a.AppointmentDate) // Sắp xếp theo ngày hẹn gần nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    CustomerName = a.Customer.FullName,
                    CustomerPhone = a.Customer.PhoneNumber ?? "Trống",
                    LicensePlate = a.Vehicle != null ? a.Vehicle.LicensePlate : "Chưa cập nhật",
                    AppointmentDate = a.AppointmentDate,
                    ExpectedServices = a.ExpectedServices,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();

            return new { TotalRecords = totalRecords, TotalPages = totalPages, CurrentPage = page, Data = data };
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> BookOnlineAsync(PublicBookingRequestDTO request)
        {
            if (request.AppointmentDate < DateTime.UtcNow)
            {
                return new { success = false, message = "Ngày hẹn không được nhỏ hơn thời gian hiện tại." };
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Tìm hoặc Tạo Khách Hàng theo SĐT
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == request.PhoneNumber);
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            FullName = request.CustomerName,
                            PhoneNumber = request.PhoneNumber,
                            Email = request.Email, // Lưu Email nếu khách có nhập
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();
                    }
                    else if (string.IsNullOrEmpty(customer.Email) && !string.IsNullOrEmpty(request.Email))
                    {
                        // Nếu khách cũ chưa có email mà lần này nhập thì cập nhật luôn
                        customer.Email = request.Email;
                        await _context.SaveChangesAsync();
                    }

                    // 2. Tìm hoặc Tạo Xe theo Biển số
                    string cleanPlate = request.LicensePlate.ToUpper().Trim();
                    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == cleanPlate);
                    if (vehicle == null)
                    {
                        vehicle = new Vehicle
                        {
                            LicensePlate = cleanPlate,
                            CustomerId = customer.Id,
                            Make = "Chưa cập nhật",
                            Model = "Chưa cập nhật"
                        };
                        _context.Vehicles.Add(vehicle);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Tạo Lịch Hẹn
                    var appointment = new Appointment
                    {
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        AppointmentDate = DateTime.SpecifyKind(request.AppointmentDate, DateTimeKind.Utc),
                        ExpectedServices = request.ExpectedServices,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // ===============================================
                    // [MỚI] GỬI EMAIL XÁC NHẬN ĐẶT LỊCH (CHẠY NGẦM)
                    // ===============================================
                    try
                    {
                        if (!string.IsNullOrEmpty(request.Email))
                        {
                            string emailBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e5e7eb; border-radius: 10px; max-width: 600px;'>
                            <h2 style='color: #2563eb; margin-bottom: 5px;'>XÁC NHẬN ĐẶT LỊCH HẸN</h2>
                            <p style='color: #64748b; font-size: 12px; margin-top: 0;'>Từ Hệ thống Smart Garage ERP</p>
                            <hr style='border-top: 1px solid #f1f5f9; margin: 20px 0;'/>
                            <p>Chào anh/chị <b>{request.CustomerName}</b>,</p>
                            <p>Cảm ơn anh/chị đã đặt lịch tại Smart Garage. Hệ thống đã ghi nhận yêu cầu bảo dưỡng cho phương tiện biển số: <b style='color: #ea580c; background: #fff7ed; padding: 3px 8px; border-radius: 5px;'>{request.LicensePlate}</b>.</p>
                            <ul style='background-color: #f8fafc; padding: 15px 15px 15px 35px; border-radius: 8px;'>
                                <li style='margin-bottom: 10px;'><b>⏰ Thời gian hẹn:</b> {request.AppointmentDate:dd/MM/yyyy - HH:mm}</li>
                                <li><b>🔧 Dịch vụ dự kiến:</b> {request.ExpectedServices}</li>
                            </ul>
                            <p>Chúng tôi sẽ ưu tiên tiếp nhận xe của anh/chị. Cố vấn dịch vụ sẽ liên hệ lại qua SĐT {request.PhoneNumber} nếu cần thêm thông tin.</p>
                            <p>Trân trọng,<br/><b>Đội ngũ Smart Garage</b></p>
                        </div>";

                            // Gọi Service gửi mail, không dùng await để tránh làm chậm luồng response của người dùng (Fire and forget)
                            _ = _emailService.SendEmailAsync(request.Email, "Xác nhận lịch hẹn tại Smart Garage", emailBody);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Chỉ log ra lỗi chứ không ném Exception làm hỏng việc đặt lịch
                        Console.WriteLine($"[Lỗi gửi mail Đặt lịch]: {ex.Message}");
                    }

                    return new { success = true, message = "Cảm ơn bạn đã đặt lịch! Gara sẽ sớm liên hệ xác nhận." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Lỗi hệ thống khi đặt lịch: " + ex.Message);
                }
            });
        }
    }
}