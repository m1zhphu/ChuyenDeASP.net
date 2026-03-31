namespace SmartGarage.DTOs
{
    public class ServiceResponseDTO
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ServiceRequestDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}