using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly GarageDbContext _context;

        public CheckInService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<CheckInResponseDTO> ProcessCheckInAsync(string licensePlate)
        {
            // Chuẩn hóa biển số: Viết hoa, xóa dấu cách, gạch ngang, dấu chấm
            string normalizedInput = licensePlate.ToUpper().Replace("-", "").Replace(".", "").Replace(" ", "");

            // Tìm xe và bao gồm thông tin khách hàng
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => (v.LicensePlate ?? "").Replace("-", "").Replace(".", "").Replace(" ", "").ToUpper() == normalizedInput);

            if (vehicle != null && vehicle.Customer != null)
            {
                // Tìm lịch hẹn trong ngày hôm nay (Dùng UtcNow để khớp với DB Render)
                var today = DateTime.UtcNow.Date;
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id
                                           && a.AppointmentDate.Date == today
                                           && a.Status != "Cancelled");

                return new CheckInResponseDTO
                {
                    IsExisting = true,
                    Message = $"Chào mừng {vehicle.Customer.FullName} quay trở lại!",
                    CustomerId = vehicle.Customer.Id,
                    CustomerName = vehicle.Customer.FullName ?? "",
                    PhoneNumber = vehicle.Customer.PhoneNumber ?? "",
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate ?? "",
                    Make = vehicle.Make ?? "",
                    Model = vehicle.Model ?? "",
                    ActiveAppointmentId = appointment?.Id // Gán ID nếu tìm thấy lịch hẹn
                };
            }

            return new CheckInResponseDTO
            {
                IsExisting = false,
                Message = "Xe mới lần đầu đến xưởng. Vui lòng tạo hồ sơ!",
                LicensePlate = licensePlate
            };
        }
    }
}