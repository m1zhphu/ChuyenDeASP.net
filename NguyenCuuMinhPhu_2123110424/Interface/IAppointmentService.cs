using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface IAppointmentService
    {
        Task<object> CreateAsync(AppointmentRequestDTO request);
        Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> UpdateStatusAsync(Guid id, string newStatus);
        Task<object> BookOnlineAsync(PublicBookingRequestDTO request);
    }
}