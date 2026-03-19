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
}