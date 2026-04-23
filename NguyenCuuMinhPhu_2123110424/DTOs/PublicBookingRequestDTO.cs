using System.ComponentModel.DataAnnotations;

namespace SmartGarage.DTOs
{
    public class PublicBookingRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập biển số xe")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string? ExpectedServices { get; set; }
    }
}