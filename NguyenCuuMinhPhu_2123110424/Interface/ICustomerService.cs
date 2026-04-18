using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync();
        Task<CustomerResponseDTO?> GetCustomerByIdAsync(Guid id);
        Task<CustomerResponseDTO> CreateCustomerAsync(CustomerRequestDTO request);
        Task<bool> UpdateCustomerAsync(Guid id, CustomerRequestDTO request);
        Task<bool> DeleteCustomerAsync(Guid id);

        // Hàm tìm kiếm khách hàng
        Task<IEnumerable<CustomerResponseDTO>> SearchCustomersAsync(string keyword);
    }
}