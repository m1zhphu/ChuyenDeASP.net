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
            // 1. Kiểm tra xe có tồn tại không
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate);
            if (vehicle == null) return new { success = false, message = "Không tìm thấy xe này trong hệ thống." };

            // 2. Tạo đối tượng Phiếu sửa chữa mới
            var order = new RepairOrder
            {
                OrderCode = $"RO-{DateTime.Now:yyyyMMddHHmm}", // Mã phiếu tự động
                VehicleId = vehicle.Id,
                AdvisorId = request.AdvisorId,
                CurrentOdometer = request.CurrentOdometer,
                Status = "InProgress",
                TotalAmount = 0
            };

            decimal total = 0;

            // 3. Xử lý phần Dịch vụ (Công thợ)
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
            }

            // 4. Xử lý phần Phụ tùng (Vật tư)
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

                    // Trừ kho
                    part.StockQuantity -= pItem.Quantity;
                    total += (part.UnitPrice * pItem.Quantity);
                }
            }

            // 5. Cập nhật tổng tiền và lưu vào Database
            order.TotalAmount = total;
            _context.RepairOrders.Add(order);
            await _context.SaveChangesAsync();

            return new { success = true, orderCode = order.OrderCode, totalAmount = order.TotalAmount };
        }
    }
}