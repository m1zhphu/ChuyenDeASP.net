using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly GarageDbContext _context;

        public AppointmentService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateAsync(AppointmentRequestDTO request)
        {
            // 1. Kiểm tra khách hàng có tồn tại không
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null) return new { success = false, message = "Không tìm thấy khách hàng." };

            // 2. Ràng buộc logic: Không cho đặt lịch trong quá khứ
            if (request.AppointmentDate < DateTime.Now)
            {
                return new { success = false, message = "Ngày hẹn không được nhỏ hơn thời gian hiện tại." };
            }

            var appointment = new Appointment
            {
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                AppointmentDate = request.AppointmentDate,
                ExpectedServices = request.ExpectedServices,
                Status = "Pending" // Mặc định khi mới đặt là chờ xác nhận
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new { success = true, id = appointment.Id, message = "Đặt lịch thành công." };
        }

        public async Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle) // Lấy kèm thông tin Xe
                .AsQueryable();

            // Logic Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            // Logic Lọc theo khoảng thời gian hẹn (Rất cần cho Lễ tân xem lịch hôm nay)
            if (fromDate.HasValue) query = query.Where(a => a.AppointmentDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(a => a.AppointmentDate <= toDate.Value);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .OrderBy(a => a.AppointmentDate) // Sắp xếp theo ngày hẹn gần nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    CustomerName = a.Customer.FullName,
                    CustomerPhone = a.Customer.PhoneNumber ?? "Trống",
                    LicensePlate = a.Vehicle != null ? a.Vehicle.LicensePlate : "Chưa cập nhật",
                    AppointmentDate = a.AppointmentDate,
                    ExpectedServices = a.ExpectedServices,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();

            return new { TotalRecords = totalRecords, TotalPages = totalPages, CurrentPage = page, Data = data };
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}