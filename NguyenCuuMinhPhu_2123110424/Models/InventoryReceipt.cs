using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class InventoryReceipt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string ReceiptCode { get; set; } = string.Empty; // VD: NK-20260409

        [Required]
        public Guid SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; } // Nhân viên lập phiếu nhập (Link tới bảng User)

        public decimal TotalAmount { get; set; } // Tổng tiền nhập hàng

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Note { get; set; }

        public virtual ICollection<InventoryReceiptDetail> ReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();
    }
}