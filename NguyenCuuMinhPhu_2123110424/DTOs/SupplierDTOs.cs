using System.ComponentModel.DataAnnotations;

namespace SmartGarage.DTOs
{
    public class SupplierResponseDTO
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class SupplierRequestDTO
    {
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc.")]
        [MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? TaxCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}