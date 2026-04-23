using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IRepairOrderService
    {
        Task<object> CreateOrderAsync(CreateRepairOrderRequest request);
        Task<RepairOrderDetailResponseDTO?> GetOrderDetailsAsync(string orderCode);
        Task<object> ProcessPaymentAsync(PaymentRequestDTO request);
        Task<IEnumerable<object>> GetAllAsync();
        Task<IEnumerable<object>> GetHistoryByLicensePlateAsync(string licensePlate);
    }
}