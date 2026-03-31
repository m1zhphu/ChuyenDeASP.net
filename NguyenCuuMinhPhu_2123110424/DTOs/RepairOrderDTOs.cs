namespace SmartGarage.DTOs
{
    public class CreateRepairOrderRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public int CurrentOdometer { get; set; }
        public Guid AdvisorId { get; set; }

        // Danh sách các mã ID dịch vụ khách chọn
        public List<Guid> ServiceIds { get; set; } = new();

        // Danh sách phụ tùng và số lượng tương ứng
        public List<PartSelectionDTO> SelectedParts { get; set; } = new();
    }

    public class PartSelectionDTO
    {
        public Guid PartId { get; set; }
        public int Quantity { get; set; }
    }
    public class RepairOrderDetailResponseDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
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
        public decimal SubTotal => Quantity * ActualPrice; // Tự động nhân ra thành tiền
    }
}