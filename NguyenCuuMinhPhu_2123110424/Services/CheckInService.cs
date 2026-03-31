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
            // Dùng Include để kéo luôn thông tin Khách hàng của chiếc xe đó lên
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

            if (vehicle != null && vehicle.Customer != null)
            {
                return new CheckInResponseDTO
                {
                    IsExisting = true,
                    Message = $"Đã tìm thấy xe. Xin chào khách hàng {vehicle.Customer.FullName}!",
                    CustomerId = vehicle.Customer.Id,
                    CustomerName = vehicle.Customer.FullName ?? string.Empty,
                    PhoneNumber = vehicle.Customer.PhoneNumber ?? string.Empty,
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate ?? string.Empty,
                    Make = vehicle.Make ?? string.Empty,
                    Model = vehicle.Model ?? string.Empty
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