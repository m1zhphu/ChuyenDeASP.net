namespace SmartGarage.DTOs
{
    // DTO dùng để trả dữ liệu về (Read)
    public class CustomerResponseDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // DTO dùng để nhận dữ liệu khi Tạo mới (Create) và Cập nhật (Update)
    public class CustomerRequestDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}