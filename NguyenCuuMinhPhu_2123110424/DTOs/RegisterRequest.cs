namespace SmartGarage.DTOs
{
    public class RegisterRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
    }
}