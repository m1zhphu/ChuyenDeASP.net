using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class PartService : IPartService
    {
        private readonly GarageDbContext _context;

        public PartService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PartResponseDTO>> GetAllAsync()
        {
            return await _context.Parts.Select(p => new PartResponseDTO
            {
                Id = p.Id,
                PartCode = p.PartCode,
                PartName = p.PartName,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity
            }).ToListAsync();
        }

        public async Task<PartResponseDTO?> GetByIdAsync(Guid id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return null;

            return new PartResponseDTO
            {
                Id = part.Id,
                PartCode = part.PartCode,
                PartName = part.PartName,
                UnitPrice = part.UnitPrice,
                StockQuantity = part.StockQuantity
            };
        }

        public async Task<PartResponseDTO> CreateAsync(PartRequestDTO request)
        {
            var part = new Part
            {
                PartCode = request.PartCode,
                PartName = request.PartName,
                UnitPrice = request.UnitPrice,
                StockQuantity = request.StockQuantity
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            return new PartResponseDTO
            {
                Id = part.Id,
                PartCode = part.PartCode,
                PartName = part.PartName,
                UnitPrice = part.UnitPrice,
                StockQuantity = part.StockQuantity
            };
        }

        public async Task<bool> UpdateAsync(Guid id, PartRequestDTO request)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return false;

            part.PartCode = request.PartCode;
            part.PartName = request.PartName;
            part.UnitPrice = request.UnitPrice;
            part.StockQuantity = request.StockQuantity;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return false;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}