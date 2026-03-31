using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IServiceItemService
    {
        Task<IEnumerable<ServiceResponseDTO>> GetAllAsync();
        Task<ServiceResponseDTO?> GetByIdAsync(Guid id);
        Task<ServiceResponseDTO> CreateAsync(ServiceRequestDTO request);
        Task<bool> UpdateAsync(Guid id, ServiceRequestDTO request);
        Task<bool> DeleteAsync(Guid id);
    }
}