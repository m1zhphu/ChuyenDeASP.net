namespace SmartGarage.DTOs
{
    public class CheckInResponseDTO
    {
        public bool IsExisting { get; set; }
        public string Message { get; set; } = string.Empty;

        // Trả về null nếu là xe mới
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid? VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}