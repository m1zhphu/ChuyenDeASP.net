using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Service
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // --- CÁC CỘT BỔ SUNG TỪ BA ---
        [MaxLength(50)]
        public string? EstimatedTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}