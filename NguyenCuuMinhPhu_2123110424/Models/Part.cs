using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Part
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string PartCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PartName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        // --- CÁC CỘT BỔ SUNG TỪ BA ---
        [MaxLength(20)]
        public string? Unit { get; set; }

        public int MinStockLevel { get; set; } = 0;

        [MaxLength(100)]
        public string? Location { get; set; }
    }
}