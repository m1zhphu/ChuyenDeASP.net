using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class ServiceItemService : IServiceItemService
    {
        private readonly GarageDbContext _context;

        public ServiceItemService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceResponseDTO>> GetAllAsync()
        {
            return await _context.Services.Select(s => new ServiceResponseDTO
            {
                Id = s.Id,
                ServiceName = s.ServiceName ?? string.Empty,
                Price = s.Price,
                Description = s.Description ?? string.Empty
            }).ToListAsync();
        }

        public async Task<ServiceResponseDTO?> GetByIdAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            return new ServiceResponseDTO
            {
                Id = service.Id,
                ServiceName = service.ServiceName ?? string.Empty,
                Price = service.Price,
                Description = service.Description ?? string.Empty
            };
        }

        public async Task<ServiceResponseDTO> CreateAsync(ServiceRequestDTO request)
        {
            var service = new Service
            {
                ServiceName = request.ServiceName ?? string.Empty,
                Price = request.Price,
                Description = request.Description ?? string.Empty
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return new ServiceResponseDTO
            {
                Id = service.Id,
                ServiceName = service.ServiceName ?? string.Empty,
                Price = service.Price,
                Description = service.Description ?? string.Empty
            };
        }

        public async Task<bool> UpdateAsync(Guid id, ServiceRequestDTO request)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            service.ServiceName = request.ServiceName ?? string.Empty;
            service.Price = request.Price;
            service.Description = request.Description ?? string.Empty;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}