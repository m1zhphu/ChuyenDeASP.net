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

        public Guid MechanicId { get; set; } // ID của thợ máy thực hiện

        public decimal ActualPrice { get; set; } // Giá lúc làm (để lưu vết nếu sau này giá dịch vụ thay đổi)
    }
}