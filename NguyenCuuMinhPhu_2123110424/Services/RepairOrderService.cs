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
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate);
            if (vehicle == null) return new { success = false, message = "Không tìm thấy xe này trong hệ thống." };

            var order = new RepairOrder
            {
                OrderCode = $"RO-{DateTime.Now:yyyyMMddHHmm}",
                VehicleId = vehicle.Id,
                AdvisorId = request.AdvisorId,
                CurrentOdometer = request.CurrentOdometer,
                Status = "InProgress",
                TotalAmount = 0
            };

            decimal total = 0;

            foreach (var sId in request.ServiceIds)
            {
                var service = await _context.Services.FindAsync(sId);
                if (service != null)
                {
                    order.OrderServices.Add(new RepairOrderServiceDetail
                    {
                        ServiceId = service.Id,
                        ActualPrice = service.Price
                    });
                    total += service.Price;
                }
                else
                {
                    // Chặn lỗi: Truyền mã dịch vụ tào lao
                    return new { success = false, message = $"Không tìm thấy dịch vụ với mã {sId} trong hệ thống." };
                }
            }

            foreach (var pItem in request.SelectedParts)
            {
                var part = await _context.Parts.FindAsync(pItem.PartId);
                if (part != null)
                {
                    if (part.StockQuantity < pItem.Quantity)
                        return new { success = false, message = $"Phụ tùng {part.PartName} không đủ số lượng trong kho." };

                    order.OrderParts.Add(new RepairOrderPartDetail
                    {
                        PartId = part.Id,
                        Quantity = pItem.Quantity,
                        ActualPrice = part.UnitPrice
                    });

                    part.StockQuantity -= pItem.Quantity;
                    total += (part.UnitPrice * pItem.Quantity);
                }
                else
                {
                    // Chặn lỗi: Truyền mã phụ tùng tào lao
                    return new { success = false, message = $"Không tìm thấy phụ tùng với mã {pItem.PartId} trong hệ thống." };
                }
            }

            order.TotalAmount = total;
            _context.RepairOrders.Add(order);
            await _context.SaveChangesAsync();

            return new { success = true, orderCode = order.OrderCode, totalAmount = order.TotalAmount };
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
                    ActualPrice = op.ActualPrice
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
                PaymentDate = DateTime.Now
            };

            // 2. Cập nhật trạng thái hóa đơn
            order.Status = "Completed";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new { success = true, message = "Thanh toán thành công. Cảm ơn quý khách!" };
        }
    }
}