using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly GarageDbContext _context;

        public VehicleService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleResponseDTO>> GetAllAsync()
        {
            return await _context.Vehicles.Select(v => new VehicleResponseDTO
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate ?? string.Empty,
                Make = v.Make ?? string.Empty,
                Model = v.Model ?? string.Empty,
                VinNumber = v.VinNumber, // Có thể Null
                CustomerId = v.CustomerId
            }).ToListAsync();
        }

        public async Task<VehicleResponseDTO?> GetByIdAsync(Guid id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v == null) return null;
            return new VehicleResponseDTO
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate ?? string.Empty,
                Make = v.Make ?? string.Empty,
                Model = v.Model ?? string.Empty,
                VinNumber = v.VinNumber, // Có thể Null
                CustomerId = v.CustomerId
            };
        }

        public async Task<object> CreateAsync(VehicleRequestDTO request)
        {
            var safeLicensePlate = request.LicensePlate ?? string.Empty;

            // Kiểm tra trùng biển số
            var exists = await _context.Vehicles.AnyAsync(v => v.LicensePlate == safeLicensePlate);
            if (exists) return new { success = false, message = "Biển số xe này đã tồn tại trong hệ thống!" };

            var vehicle = new Vehicle
            {
                LicensePlate = safeLicensePlate,
                Make = request.Make ?? string.Empty,
                Model = request.Model ?? string.Empty,
                VinNumber = request.VinNumber,
                CustomerId = request.CustomerId
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return new { success = true, data = new VehicleResponseDTO { Id = vehicle.Id, LicensePlate = vehicle.LicensePlate ?? string.Empty, Make = vehicle.Make ?? string.Empty, Model = vehicle.Model ?? string.Empty, VinNumber = vehicle.VinNumber, CustomerId = vehicle.CustomerId } };
        }

        public async Task<object> UpdateAsync(Guid id, VehicleRequestDTO request)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return new { success = false, message = "Không tìm thấy xe." };

            var safeLicensePlate = request.LicensePlate ?? string.Empty;

            // Kiểm tra trùng biển số (ngoại trừ chính chiếc xe đang sửa)
            var duplicate = await _context.Vehicles.AnyAsync(v => v.LicensePlate == safeLicensePlate && v.Id != id);
            if (duplicate) return new { success = false, message = "Biển số xe mới bị trùng với xe khác!" };

            vehicle.LicensePlate = safeLicensePlate;
            vehicle.Make = request.Make ?? string.Empty;
            vehicle.Model = request.Model ?? string.Empty;
            vehicle.VinNumber = request.VinNumber;
            vehicle.CustomerId = request.CustomerId;

            await _context.SaveChangesAsync();
            return new { success = true, message = "Cập nhật thành công." };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}