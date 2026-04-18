namespace SmartGarage.DTOs
{
    public class DashboardStatsDTO
    {
        public int TotalCustomers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalRepairOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RecentOrderDTO> RecentOrders { get; set; } = new();
        public List<LowStockPartDTO> LowStockParts { get; set; } = new();
    }

    public class RecentOrderDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LowStockPartDTO
    {
        public string PartName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
    }
}