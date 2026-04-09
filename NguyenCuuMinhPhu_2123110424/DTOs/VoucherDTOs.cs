using System.ComponentModel.DataAnnotations;

namespace SmartGarage.DTOs
{
    public class VoucherRequestDTO
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [MaxLength(50)]
        public string VoucherCode { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100.")]
        public decimal DiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa không hợp lệ.")]
        public decimal MaxDiscountAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 10000)]
        public int UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class VoucherResponseDTO
    {
        public Guid Id { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }

        // Trả về thêm cờ này để Frontend biết mã còn dùng được hay không
        public bool IsValid => IsActive && StartDate <= DateTime.Now && EndDate >= DateTime.Now && UsedCount < UsageLimit;
    }
}