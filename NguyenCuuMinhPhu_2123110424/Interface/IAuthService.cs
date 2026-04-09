using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IAuthService
    {
        Task<object> LoginAsync(LoginRequestDTO request);
    }
}