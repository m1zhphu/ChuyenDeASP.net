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

        public RepairOrderService(GarageDbContext context)
        {
            _context = context;
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
                // Thay Now thành UtcNow
                PaymentDate = DateTime.UtcNow
            };

            // 2. Cập nhật trạng thái hóa đơn
            order.Status = "Completed";

            // 3. Logic lưu vết: Cập nhật ngày bảo dưỡng cuối cùng cho xe
            var vehicle = await _context.Vehicles.FindAsync(order.VehicleId);
            if (vehicle != null)
            {
                // Thay Now thành UtcNow
                vehicle.LastServiceDate = DateTime.UtcNow;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

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