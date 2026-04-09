using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly GarageDbContext _context;

        public VoucherService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateAsync(VoucherRequestDTO request)
        {
            // Kiểm tra mã trùng
            var exists = await _context.Vouchers.AnyAsync(v => v.VoucherCode.ToUpper() == request.VoucherCode.ToUpper());
            if (exists) return new { success = false, message = "Mã khuyến mãi này đã tồn tại!" };

            if (request.EndDate <= request.StartDate)
                return new { success = false, message = "Ngày kết thúc phải lớn hơn ngày bắt đầu." };

            var voucher = new Voucher
            {
                VoucherCode = request.VoucherCode.ToUpper(), // Tự động viết hoa toàn bộ mã
                DiscountPercent = request.DiscountPercent,
                MaxDiscountAmount = request.MaxDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = request.IsActive,
                UsedCount = 0
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return new { success = true, id = voucher.Id, message = "Tạo mã khuyến mãi thành công." };
        }

        public async Task<object> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.Vouchers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v => v.VoucherCode.Contains(search.ToUpper()));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .OrderByDescending(v => v.CreatedAt) // Mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VoucherResponseDTO
                {
                    Id = v.Id,
                    VoucherCode = v.VoucherCode,
                    DiscountPercent = v.DiscountPercent,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    UsageLimit = v.UsageLimit,
                    UsedCount = v.UsedCount,
                    IsActive = v.IsActive
                }).ToListAsync();

            return new { TotalRecords = totalRecords, TotalPages = totalPages, CurrentPage = page, Data = data };
        }

        public async Task<object> ValidateVoucherAsync(string code)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == code.ToUpper());

            if (voucher == null)
                return new { isValid = false, message = "Mã khuyến mãi không tồn tại." };

            if (!voucher.IsActive)
                return new { isValid = false, message = "Mã khuyến mãi đã bị khóa." };

            if (voucher.StartDate > DateTime.Now)
                return new { isValid = false, message = "Mã khuyến mãi chưa đến thời gian sử dụng." };

            if (voucher.EndDate < DateTime.Now)
                return new { isValid = false, message = "Mã khuyến mãi đã hết hạn." };

            if (voucher.UsedCount >= voucher.UsageLimit)
                return new { isValid = false, message = "Mã khuyến mãi đã hết lượt sử dụng." };

            // Nếu lọt qua hết các chốt chặn trên, trả về thông tin để API Tính tiền tính toán
            return new
            {
                isValid = true,
                discountPercent = voucher.DiscountPercent,
                maxDiscountAmount = voucher.MaxDiscountAmount,
                message = "Áp dụng mã thành công!"
            };
        }
    }
}