using System.ComponentModel.DataAnnotations;

namespace SmartGarage.DTOs
{
    public class AppointmentRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng.")]
        public Guid CustomerId { get; set; }

        public Guid? VehicleId { get; set; } // Có thể null nếu khách đi xe mới chưa từng lưu vào hệ thống

        [Required(ErrorMessage = "Vui lòng chọn ngày giờ hẹn.")]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string? ExpectedServices { get; set; }
    }

    public class AppointmentStatusUpdateDTO
    {
        [Required]
        [RegularExpression("Pending|Confirmed|Cancelled|Arrived", ErrorMessage = "Trạng thái không hợp lệ. Chỉ chấp nhận: Pending, Confirmed, Cancelled, Arrived")]
        public string Status { get; set; } = string.Empty;
    }

    public class AppointmentResponseDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? LicensePlate { get; set; } // Biển số xe (nếu có)
        public DateTime AppointmentDate { get; set; }
        public string? ExpectedServices { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}