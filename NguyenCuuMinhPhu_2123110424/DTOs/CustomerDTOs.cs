namespace SmartGarage.DTOs
{
    public class CustomerResponseDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerRequestDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
    }
}