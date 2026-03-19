using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class RepairOrder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string OrderCode { get; set; } = string.Empty; // Mã phiếu in ra (VD: RO-2026) [cite: 129]

        public Guid VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        public Guid AdvisorId { get; set; } // Cố vấn lập phiếu (Link tới bảng User) [cite: 131]

        public int CurrentOdometer { get; set; } // Số Km hiện tại của xe [cite: 132]

        public string Status { get; set; } = "Pending"; // Trạng thái phiếu [cite: 133]

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ 1-nhiều tới chi tiết công thợ và phụ tùng
        public virtual ICollection<RepairOrderServiceDetail> OrderServices { get; set; } = new List<RepairOrderServiceDetail>();
        public virtual ICollection<RepairOrderPartDetail> OrderParts { get; set; } = new List<RepairOrderPartDetail>();
    }
}