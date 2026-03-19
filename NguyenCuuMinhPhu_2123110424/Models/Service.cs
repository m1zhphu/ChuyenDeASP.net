using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Service
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; } // Đơn giá dịch vụ [cite: 114]

        public string? Description { get; set; }
    }
}