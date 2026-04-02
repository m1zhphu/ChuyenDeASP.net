using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Make { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }

        [MaxLength(50)]
        public string? VinNumber { get; set; }

        // --- CỘT BỔ SUNG TỪ BA ---
        public DateTime? LastServiceDate { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}