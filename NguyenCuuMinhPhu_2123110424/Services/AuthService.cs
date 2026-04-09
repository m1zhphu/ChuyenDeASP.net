using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartGarage.Services
{
    public class AuthService : IAuthService
    {
        private readonly GarageDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(GarageDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Dùng để đọc key bí mật từ appsettings
        }

        public async Task<object> LoginAsync(LoginRequestDTO request)
        {
            // 1. Tìm User trong Database (Lưu ý: Thực tế Password phải được Hash, ở đây mình làm đơn giản kiểm tra string để bạn dễ test)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null) return new { success = false, message = "Tài khoản hoặc mật khẩu không chính xác." };
            if (!user.IsActive) return new { success = false, message = "Tài khoản này đã bị khóa." };

            // Cập nhật lần đăng nhập cuối
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // 2. Tạo Claims (Những thông tin được nhúng vào trong Token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role) // Rất quan trọng để phân quyền (Admin, Advisor...)
            };

            // 3. Đọc Key bí mật từ cấu hình để ký xác nhận Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Định hình Token (Thời hạn 1 ngày)
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Trả về kết quả
            return new
            {
                success = true,
                data = new LoginResponseDTO { UserId = user.Id, FullName = user.FullName, Role = user.Role, Token = jwt }
            };
        }
    }
}