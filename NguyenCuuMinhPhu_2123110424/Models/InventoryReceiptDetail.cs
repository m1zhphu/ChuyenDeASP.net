using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class InventoryReceiptDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReceiptId { get; set; }
        [ForeignKey("ReceiptId")]
        public virtual InventoryReceipt InventoryReceipt { get; set; } = null!;

        [Required]
        public Guid PartId { get; set; }
        [ForeignKey("PartId")]
        public virtual Part Part { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } // Số lượng nhập

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; } // Giá nhập vốn
    }
}