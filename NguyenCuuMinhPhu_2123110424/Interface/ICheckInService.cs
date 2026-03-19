using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface ICheckInService
    {
        Task<object> ProcessScanAsync(string licensePlate);
        Task<bool> RegisterCustomerAsync(RegisterRequest request);
    }
}