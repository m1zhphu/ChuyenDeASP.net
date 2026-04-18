namespace SmartGarage.DTOs
{
    public class CheckInResponseDTO
    {
        public bool IsExisting { get; set; }
        public string Message { get; set; } = string.Empty; // Khởi tạo để tránh warning
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        // Thuộc tính mới để sửa lỗi "does not contain a definition"
        public Guid? ActiveAppointmentId { get; set; }
    }
}