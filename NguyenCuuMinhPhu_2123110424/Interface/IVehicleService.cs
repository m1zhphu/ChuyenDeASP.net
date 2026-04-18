using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponseDTO>> GetAllAsync();
        Task<VehicleResponseDTO?> GetByIdAsync(Guid id);
        // Dùng Task<object> để dễ dàng trả về thông báo lỗi nếu trùng biển số
        Task<object> CreateAsync(VehicleRequestDTO request);
        Task<object> UpdateAsync(Guid id, VehicleRequestDTO request);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<VehicleResponseDTO>> SearchAsync(string keyword);
    }
}