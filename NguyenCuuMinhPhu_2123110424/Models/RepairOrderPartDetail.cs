using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class RepairOrderPartDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RepairOrderId { get; set; }
        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; } = null!;

        public Guid PartId { get; set; }
        [ForeignKey("PartId")]
        public virtual Part Part { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal ActualPrice { get; set; }

        // --- CÁC CỘT BỔ SUNG TỪ BA ---
        [MaxLength(255)]
        public string? Note { get; set; }

        public decimal DiscountPercent { get; set; } = 0;
    }
}