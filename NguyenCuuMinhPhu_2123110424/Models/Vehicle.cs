using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty; // Biển số xe quét từ YOLO

        [MaxLength(50)]
        public string? Make { get; set; } // Hãng xe (VD: Toyota)

        [MaxLength(50)]
        public string? Model { get; set; } // Dòng xe (VD: Vios)

        // Khóa ngoại liên kết với bảng Customer
        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}