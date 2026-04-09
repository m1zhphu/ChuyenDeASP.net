using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IUserService
    {
        Task<object> GetPagedAsync(int page, int pageSize, string? search);
        Task<object> CreateAsync(UserRequestDTO request);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO request);
    }
}