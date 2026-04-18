using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class RepairOrderServiceDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RepairOrderId { get; set; }
        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; } = null!;

        public Guid ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; } = null!;

        public Guid? MechanicId { get; set; } // Đổi thành Guid? (nullable) vì lúc mới tạo phiếu có thể chưa phân thợ ngay

        public decimal ActualPrice { get; set; }
        // THÊM TRƯỜNG NÀY: Để quản lý trạng thái từng Task
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
    }
}