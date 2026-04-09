using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IVoucherService
    {
        Task<object> CreateAsync(VoucherRequestDTO request);
        Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null);

        // Hàm cực kỳ quan trọng: Kiểm tra mã có xài được không
        Task<object> ValidateVoucherAsync(string code);
    }
}