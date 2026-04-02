namespace SmartGarage.DTOs
{
    public class CreateRepairOrderRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public int CurrentOdometer { get; set; }
        public Guid AdvisorId { get; set; }

        // Các trường mới cho hóa đơn
        public DateTime? ExpectedDeliveryTime { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public string? Note { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();
        public List<PartSelectionDTO> SelectedParts { get; set; } = new();
    }

    public class PartSelectionDTO
    {
        public Guid PartId { get; set; }
        public int Quantity { get; set; }

        // Thêm ghi chú và giảm giá riêng cho từng linh kiện
        public string? Note { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
    }

    public class RepairOrderDetailResponseDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        // Cập nhật cấu trúc tiền bạc
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public DateTime? ExpectedDeliveryTime { get; set; }
        public string? Note { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleInfo { get; set; } = string.Empty;

        public List<ServiceDetailDTO> Services { get; set; } = new List<ServiceDetailDTO>();
        public List<PartDetailDTO> Parts { get; set; } = new List<PartDetailDTO>();
    }

    public class ServiceDetailDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal ActualPrice { get; set; }
    }

    public class PartDetailDTO
    {
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ActualPrice { get; set; }
        public string? Note { get; set; }
        public decimal DiscountPercent { get; set; }

        // Tự động tính thành tiền sau khi trừ % giảm giá
        public decimal SubTotal => (Quantity * ActualPrice) * (1 - DiscountPercent / 100m);
    }
}