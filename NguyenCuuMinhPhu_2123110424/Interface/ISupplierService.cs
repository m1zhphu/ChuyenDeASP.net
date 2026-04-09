using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface ISupplierService
    {
        // Sử dụng kiểu trả về object để chứa cả Data và thông tin Phân trang
        Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<SupplierResponseDTO?> GetByIdAsync(Guid id);
        Task<SupplierResponseDTO> CreateAsync(SupplierRequestDTO request);
        Task<bool> UpdateAsync(Guid id, SupplierRequestDTO request);
        Task<object> DeleteAsync(Guid id); // Đổi thành object để trả về câu báo lỗi nếu đang dính khóa ngoại
    }
}