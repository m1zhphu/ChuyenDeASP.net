using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IPartService
    {
        Task<IEnumerable<PartResponseDTO>> GetAllAsync();
        Task<PartResponseDTO?> GetByIdAsync(Guid id);
        Task<PartResponseDTO> CreateAsync(PartRequestDTO request);
        Task<bool> UpdateAsync(Guid id, PartRequestDTO request);
        Task<bool> DeleteAsync(Guid id);
    }
}