namespace SmartGarage.DTOs
{
    public class QuickOnboardRequestDTO
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
    }
}