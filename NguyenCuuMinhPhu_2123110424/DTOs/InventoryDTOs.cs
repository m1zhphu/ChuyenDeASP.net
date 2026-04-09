using System.ComponentModel.DataAnnotations;

namespace SmartGarage.DTOs
{
    // Request khi bấm nút "Lưu Phiếu Nhập"
    public class CreateReceiptRequestDTO
    {
        [Required(ErrorMessage = "Mã phiếu nhập là bắt buộc.")]
        [MaxLength(50)]
        public string ReceiptCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
        public Guid SupplierId { get; set; }

        [Required(ErrorMessage = "Vui lòng truyền ID người lập phiếu.")]
        public Guid UserId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phiếu nhập phải có ít nhất 1 mặt hàng.")]
        public List<ReceiptDetailRequestDTO> Details { get; set; } = new();
    }

    public class ReceiptDetailRequestDTO
    {
        [Required]
        public Guid PartId { get; set; }

        [Range(1, 10000, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không được là số âm.")]
        public decimal ImportPrice { get; set; }
    }

    // Response trả về khi xem chi tiết phiếu nhập
    public class ReceiptResponseDTO
    {
        public Guid Id { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty; // Tên nhân viên nhập
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }

        public List<ReceiptItemResponseDTO> Items { get; set; } = new();
    }

    public class ReceiptItemResponseDTO
    {
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SubTotal => Quantity * ImportPrice;
    }
}