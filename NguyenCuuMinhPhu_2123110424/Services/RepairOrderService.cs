using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly GarageDbContext _context;
        private readonly IEmailService _emailService;
        public RepairOrderService(GarageDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<object> CreateOrderAsync(CreateRepairOrderRequest request)
        {
            // Bọc Transaction trong ExecutionStrategy để tương thích với EnableRetryOnFailure
            var strategy = _context.Database.CreateExecutionStrategy();

            // ĐÃ SỬA LỖI: Thêm <object> để C# hiểu kiểu dữ liệu trả về
            return await strategy.ExecuteAsync<object>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate);
                    if (vehicle == null) return new { success = false, message = "Không tìm thấy xe này trong hệ thống." };

                    var order = new RepairOrder
                    {
                        OrderCode = $"RO-{DateTime.UtcNow:yyyyMMddHHmm}",
                        VehicleId = vehicle.Id,
                        AdvisorId = request.AdvisorId,
                        CurrentOdometer = request.CurrentOdometer,
                        Status = "InProgress",
                        TotalAmount = 0,
                        ExpectedDeliveryTime = request.ExpectedDeliveryTime.HasValue
                            ? DateTime.SpecifyKind(request.ExpectedDeliveryTime.Value, DateTimeKind.Utc)
                            : null,
                        DiscountAmount = request.DiscountAmount,
                        TaxAmount = request.TaxAmount,
                        Note = request.Note,
                        CreatedAt = DateTime.UtcNow
                    };

                    decimal total = 0;

                    // 1. Xử lý Dịch vụ & Giao việc
                    foreach (var sItem in request.ServiceIds)
                    {
                        var service = await _context.Services.FindAsync(sItem.ServiceId);
                        if (service != null)
                        {
                            order.OrderServices.Add(new RepairOrderServiceDetail
                            {
                                ServiceId = service.Id,
                                MechanicId = sItem.MechanicId,
                                ActualPrice = service.Price,
                                Status = "Pending"
                            });
                            total += service.Price;
                        }
                    }

                    // 2. Xử lý Phụ tùng
                    foreach (var pItem in request.SelectedParts)
                    {
                        var part = await _context.Parts.FindAsync(pItem.PartId);
                        if (part != null)
                        {
                            if (part.StockQuantity < pItem.Quantity)
                            {
                                await transaction.RollbackAsync();
                                return new { success = false, message = $"Phụ tùng {part.PartName} không đủ kho." };
                            }

                            order.OrderParts.Add(new RepairOrderPartDetail
                            {
                                PartId = part.Id,
                                Quantity = pItem.Quantity,
                                ActualPrice = part.UnitPrice,
                                Note = pItem.Note,
                                DiscountPercent = pItem.DiscountPercent
                            });

                            part.StockQuantity -= pItem.Quantity;
                            total += (part.UnitPrice * pItem.Quantity) * (1 - pItem.DiscountPercent / 100m);
                        }
                    }

                    order.TotalAmount = total;
                    order.FinalAmount = order.TotalAmount - order.DiscountAmount + order.TaxAmount;

                    _context.RepairOrders.Add(order);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new { success = true, orderCode = order.OrderCode, finalAmount = order.FinalAmount };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new { success = false, message = "Lỗi hệ thống: " + ex.Message };
                }
            });
        }

        public async Task<RepairOrderDetailResponseDTO?> GetOrderDetailsAsync(string orderCode)
        {
            var order = await _context.RepairOrders
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(ro => ro.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(ro => ro.OrderParts)
                    .ThenInclude(op => op.Part)
                .FirstOrDefaultAsync(ro => ro.OrderCode == orderCode);

            if (order == null || order.Vehicle == null || order.Vehicle.Customer == null) return null;

            return new RepairOrderDetailResponseDTO
            {
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                FinalAmount = order.FinalAmount,
                ExpectedDeliveryTime = order.ExpectedDeliveryTime,
                Note = order.Note,
                CustomerName = order.Vehicle.Customer.FullName ?? string.Empty,
                PhoneNumber = order.Vehicle.Customer.PhoneNumber ?? string.Empty,
                LicensePlate = order.Vehicle.LicensePlate ?? string.Empty,
                VehicleInfo = $"{order.Vehicle.Make} {order.Vehicle.Model}",
                Services = order.OrderServices.Select(os => new ServiceDetailDTO
                {
                    ServiceName = os.Service?.ServiceName ?? "Dịch vụ không xác định",
                    ActualPrice = os.ActualPrice
                }).ToList(),
                Parts = order.OrderParts.Select(op => new PartDetailDTO
                {
                    PartName = op.Part?.PartName ?? "Phụ tùng không xác định",
                    Quantity = op.Quantity,
                    ActualPrice = op.ActualPrice,
                    Note = op.Note,
                    DiscountPercent = op.DiscountPercent
                }).ToList()
            };
        }

        public async Task<object> ProcessPaymentAsync(PaymentRequestDTO request)
        {
            var order = await _context.RepairOrders
                .FirstOrDefaultAsync(ro => ro.OrderCode == request.OrderCode);

            if (order == null) return new { success = false, message = "Không tìm thấy hóa đơn." };

            if (order.Status == "Completed") return new { success = false, message = "Hóa đơn này đã được thanh toán trước đó." };

            // 1. Lưu thông tin thanh toán
            var payment = new Payment
            {
                RepairOrderId = order.Id,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = DateTime.UtcNow
            };

            // 2. Cập nhật trạng thái hóa đơn
            order.Status = "Completed";

            // 3. Logic lưu vết: Cập nhật ngày bảo dưỡng cuối cùng cho xe
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer) // Include Customer để lát nữa lấy Email gửi Hóa đơn
                .FirstOrDefaultAsync(v => v.Id == order.VehicleId);

            if (vehicle != null)
            {
                vehicle.LastServiceDate = DateTime.UtcNow;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // ===============================================
            // [MỚI] GỬI HÓA ĐƠN ĐIỆN TỬ QUA EMAIL KHÁCH HÀNG
            // ===============================================
            try
            {
                string? customerEmail = vehicle?.Customer?.Email;

                if (!string.IsNullOrEmpty(customerEmail))
                {
                    // Gọi lại hàm GetOrderDetailsAsync (đã có sẵn trong file này) để lấy chi tiết dịch vụ/phụ tùng
                    var details = await GetOrderDetailsAsync(request.OrderCode);

                    if (details != null)
                    {
                        // Thay localhost:5173 bằng domain thực tế khi bạn đưa lên mạng
                        string traCuuUrl = $"http://localhost:5173/tra-cuu/{details.LicensePlate}";

                        string invoiceHtml = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e5e7eb; border-radius: 10px; max-width: 600px;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h2 style='color: #16a34a; margin-bottom: 5px;'>HÓA ĐƠN ĐIỆN TỬ</h2>
                            <p style='color: #64748b; font-size: 14px; margin-top: 0;'>SMART GARAGE ERP</p>
                        </div>
                        
                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>
                            <p style='margin: 5px 0;'><b>Khách hàng:</b> {details.CustomerName}</p>
                            <p style='margin: 5px 0;'><b>Mã phiếu:</b> {details.OrderCode}</p>
                            <p style='margin: 5px 0;'><b>Biển số xe:</b> {details.LicensePlate}</p>
                            <p style='margin: 5px 0;'><b>Hoàn thành:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
                        </div>

                        <h3 style='color: #1e293b; border-bottom: 2px solid #e2e8f0; padding-bottom: 5px;'>TỔNG THANH TOÁN: <span style='color: #2563eb; font-size: 24px;'>{details.FinalAmount:N0} VNĐ</span></h3>
                        
                        <div style='text-align: center; margin-top: 30px; padding: 20px; background-color: #f0fdf4; border-radius: 8px;'>
                            <p style='color: #166534; font-weight: bold; margin-bottom: 15px;'>Mời quý khách xem chi tiết các hạng mục thay thế tại Sổ bảo dưỡng điện tử:</p>
                            <a href='{traCuuUrl}' style='background-color: #16a34a; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>TRA CỨU SỔ BẢO DƯỠNG SỐ</a>
                        </div>
                        
                        <p style='font-size: 11px; color: #94a3b8; text-align: center; margin-top: 20px;'>
                            Email này được gửi tự động từ hệ thống Smart Garage. Vui lòng không trả lời.
                        </p>
                    </div>";

                        // Fire and forget
                        _ = _emailService.SendEmailAsync(customerEmail, $"Hóa đơn thanh toán Dịch vụ - Phiếu {details.OrderCode}", invoiceHtml);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi gửi mail Hóa đơn]: {ex.Message}");
            }

            return new { success = true, message = "Thanh toán thành công. Cảm ơn quý khách!" };
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            // Nối bảng để lấy thêm Tên Khách và Biển số ra màn hình danh sách
            return await _context.RepairOrders
                .Include(ro => ro.Vehicle)
                    .ThenInclude(v => v.Customer)
                .OrderByDescending(ro => ro.CreatedAt) // Sắp xếp lệnh mới nhất lên đầu
                .Select(ro => new
                {
                    id = ro.Id,
                    orderCode = ro.OrderCode,
                    licensePlate = ro.Vehicle != null ? ro.Vehicle.LicensePlate : "---",
                    customerName = (ro.Vehicle != null && ro.Vehicle.Customer != null) ? ro.Vehicle.Customer.FullName : "Khách lẻ",
                    totalAmount = ro.TotalAmount,
                    finalAmount = ro.FinalAmount,
                    status = ro.Status,
                    isPaid = ro.Status == "Completed"
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<object>> GetHistoryByLicensePlateAsync(string licensePlate)
        {
            string normalizedPlate = licensePlate.ToUpper().Replace("-", "").Replace(".", "").Replace(" ", "");

            return await _context.RepairOrders
                .Include(ro => ro.Vehicle)
                .Include(ro => ro.OrderServices).ThenInclude(os => os.Service)
                .Include(ro => ro.OrderParts).ThenInclude(op => op.Part)
                .Where(ro => (ro.Vehicle.LicensePlate ?? "").Replace("-", "").Replace(".", "").Replace(" ", "").ToUpper() == normalizedPlate
                       && ro.Status == "Completed")
                .OrderByDescending(ro => ro.CreatedAt)
                .Select(ro => new
                {
                    orderCode = ro.OrderCode,
                    createdAt = ro.CreatedAt,
                    currentOdometer = ro.CurrentOdometer,
                    finalAmount = ro.FinalAmount, // <--- ĐẢM BẢO CÓ DÒNG NÀY ĐỂ HIỂN THỊ GIÁ
                    services = ro.OrderServices.Select(s => new { serviceName = s.Service.ServiceName }).ToList(),
                    parts = ro.OrderParts.Select(p => new { partName = p.Part.PartName, quantity = p.Quantity }).ToList()
                })
                .ToListAsync();
        }
    }
}