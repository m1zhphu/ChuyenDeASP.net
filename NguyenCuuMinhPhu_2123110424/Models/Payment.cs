using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RepairOrderId { get; set; }
        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; } = null!;

        public decimal AmountPaid { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}