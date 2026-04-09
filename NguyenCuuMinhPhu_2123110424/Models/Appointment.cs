using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGarage.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        public Guid? VehicleId { get; set; } // Có thể khách mới chưa có xe trong hệ thống

        [Required]
        public DateTime AppointmentDate { get; set; } // Ngày giờ khách hẹn tới

        [MaxLength(500)]
        public string? ExpectedServices { get; set; } // Khách ghi chú muốn làm gì (VD: Thay nhớt, kiểm tra phanh)

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Arrived

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}