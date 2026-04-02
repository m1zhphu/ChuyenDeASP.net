namespace SmartGarage.DTOs
{
    public class VehicleResponseDTO
    {
        public Guid Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? VinNumber { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime? LastServiceDate { get; set; }
    }

    public class VehicleRequestDTO
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? VinNumber { get; set; }
        public Guid CustomerId { get; set; }
    }
}