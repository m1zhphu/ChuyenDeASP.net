using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;

namespace SmartGarage.Services
{
    public class DashboardService
    {
        private readonly GarageDbContext _context;

        public DashboardService(GarageDbContext context) => _context = context;

        public async Task<DashboardStatsDTO> GetStatsAsync()
        {
            return new DashboardStatsDTO
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                TotalRepairOrders = await _context.RepairOrders.CountAsync(),
                // Tính tổng doanh thu từ các lệnh đã thanh toán/hoàn thành
                TotalRevenue = await _context.RepairOrders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.FinalAmount),

                RecentOrders = await _context.RepairOrders
                    .Include(o => o.Vehicle).ThenInclude(v => v.Customer)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOrderDTO
                    {
                        OrderCode = o.OrderCode,
                        CustomerName = o.Vehicle.Customer.FullName ?? "Khách lẻ",
                        FinalAmount = o.FinalAmount,
                        Status = o.Status
                    }).ToListAsync(),

                LowStockParts = await _context.Parts
                    .Where(p => p.StockQuantity <= p.MinStockLevel)
                    .Select(p => new LowStockPartDTO
                    {
                        // Sửa tất cả chữ cái đầu thành CHỮ HOA
                        PartName = p.PartName,
                        StockQuantity = p.StockQuantity,
                        MinStockLevel = p.MinStockLevel
                    }).ToListAsync()
            };
        }
    }
}