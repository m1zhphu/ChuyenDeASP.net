using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly GarageDbContext _context;

        public SupplierService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.Suppliers.AsQueryable();

            // Logic Tìm kiếm: Tìm theo Tên hoặc Số điện thoại
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.SupplierName.Contains(search) || (s.PhoneNumber != null && s.PhoneNumber.Contains(search)));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Logic Phân trang
            var data = await query
                .OrderByDescending(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierResponseDTO
                {
                    Id = s.Id,
                    SupplierName = s.SupplierName,
                    PhoneNumber = s.PhoneNumber,
                    Address = s.Address,
                    TaxCode = s.TaxCode,
                    IsActive = s.IsActive
                }).ToListAsync();

            return new
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = data
            };
        }

        public async Task<SupplierResponseDTO?> GetByIdAsync(Guid id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return null;

            return new SupplierResponseDTO
            {
                Id = s.Id,
                SupplierName = s.SupplierName,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                TaxCode = s.TaxCode,
                IsActive = s.IsActive
            };
        }

        public async Task<SupplierResponseDTO> CreateAsync(SupplierRequestDTO request)
        {
            var supplier = new Supplier
            {
                SupplierName = request.SupplierName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                TaxCode = request.TaxCode,
                IsActive = request.IsActive
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return new SupplierResponseDTO { Id = supplier.Id, SupplierName = supplier.SupplierName };
        }

        public async Task<bool> UpdateAsync(Guid id, SupplierRequestDTO request)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return false;

            supplier.SupplierName = request.SupplierName;
            supplier.PhoneNumber = request.PhoneNumber;
            supplier.Address = request.Address;
            supplier.TaxCode = request.TaxCode;
            supplier.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> DeleteAsync(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return new { success = false, message = "Không tìm thấy nhà cung cấp." };

            // Kiểm tra xem Nhà cung cấp này đã có Phiếu nhập kho nào chưa
            var hasReceipts = await _context.InventoryReceipts.AnyAsync(r => r.SupplierId == id);
            if (hasReceipts)
            {
                return new { success = false, message = "Không thể xóa! Nhà cung cấp này đã có giao dịch nhập kho. Vui lòng chuyển trạng thái IsActive = false." };
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return new { success = true, message = "Đã xóa nhà cung cấp thành công." };
        }
    }
}