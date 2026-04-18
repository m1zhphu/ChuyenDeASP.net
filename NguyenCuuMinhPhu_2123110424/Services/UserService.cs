using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class UserService : IUserService
    {
        private readonly GarageDbContext _context;

        public UserService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetPagedAsync(int page, int pageSize, string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Username.Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    LastLogin = u.LastLogin
                }).ToListAsync();

            return new { TotalRecords = total, Data = data };
        }

        public async Task<object> CreateAsync(UserRequestDTO request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return new { success = false, message = "Tên đăng nhập đã tồn tại." };

            var user = new User
            {
                Username = request.Username,
                PasswordHash = request.Password, // Mapping sang field PasswordHash trong Model
                FullName = request.FullName,
                Role = request.Role,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new { success = true, message = "Tạo tài khoản thành công." };
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.PasswordHash != request.OldPassword) return false;

            user.PasswordHash = request.NewPassword;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}