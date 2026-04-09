using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Voucher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string VoucherCode { get; set; } = string.Empty; // VD: TRIAN2026

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } // Giảm %

        public decimal MaxDiscountAmount { get; set; } // Giảm tối đa bao nhiêu tiền

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int UsageLimit { get; set; } = 100; // Giới hạn số lần dùng

        public int UsedCount { get; set; } = 0; // Đã dùng bao nhiêu lần

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}