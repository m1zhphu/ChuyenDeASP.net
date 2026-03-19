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

        public decimal UnitPrice { get; set; } // Giá bán lẻ [cite: 121]

        public int StockQuantity { get; set; } // Số lượng tồn kho [cite: 122]
    }
}