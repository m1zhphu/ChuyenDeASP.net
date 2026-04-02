using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class RepairOrder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        public Guid VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        public Guid AdvisorId { get; set; }

        public int CurrentOdometer { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- CÁC CỘT BỔ SUNG TỪ BA ---
        public DateTime? ExpectedDeliveryTime { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        public decimal TaxAmount { get; set; } = 0;

        public decimal FinalAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(255)]
        public string? CancellationReason { get; set; }

        public virtual ICollection<RepairOrderServiceDetail> OrderServices { get; set; } = new List<RepairOrderServiceDetail>();
        public virtual ICollection<RepairOrderPartDetail> OrderParts { get; set; } = new List<RepairOrderPartDetail>();
    }
}