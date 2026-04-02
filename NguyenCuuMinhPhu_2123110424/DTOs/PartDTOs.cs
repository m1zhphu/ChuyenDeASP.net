namespace SmartGarage.DTOs
{
    public class PartResponseDTO
    {
        public Guid Id { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Unit { get; set; }
        public int MinStockLevel { get; set; }
        public string? Location { get; set; }
    }

    public class PartRequestDTO
    {
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Unit { get; set; }
        public int MinStockLevel { get; set; }
        public string? Location { get; set; }
    }
}