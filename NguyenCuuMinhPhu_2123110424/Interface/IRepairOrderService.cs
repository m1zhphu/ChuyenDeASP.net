using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IRepairOrderService
    {
        Task<object> CreateOrderAsync(CreateRepairOrderRequest request);
    }
}