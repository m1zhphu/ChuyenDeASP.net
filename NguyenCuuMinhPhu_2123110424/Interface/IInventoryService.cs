using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IInventoryService
    {
        Task<object> CreateReceiptAsync(CreateReceiptRequestDTO request);
        Task<object> GetPagedReceiptsAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<ReceiptResponseDTO?> GetReceiptDetailsAsync(Guid id);
    }
}