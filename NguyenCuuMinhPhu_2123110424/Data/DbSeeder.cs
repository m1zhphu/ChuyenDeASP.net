using SmartGarage.Models;

namespace SmartGarage.Data
{
    public static class DbSeeder
    {
        public static void Seed(GarageDbContext context)
        {
            // 1. Nạp dịch vụ mẫu
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { ServiceName = "Thay nhớt động cơ", Price = 150000, Description = "Công thay nhớt và kiểm tra tổng quát" },
                    new Service { ServiceName = "Vệ sinh thắng (phanh)", Price = 200000 },
                    new Service { ServiceName = "Cân chỉnh thước lái", Price = 500000 }
                );
            }

            // 2. Nạp phụ tùng mẫu
            if (!context.Parts.Any())
            {
                context.Parts.AddRange(
                    new Part { PartCode = "OIL-5W30", PartName = "Nhớt Castrol 5W-30 (4L)", UnitPrice = 850000, StockQuantity = 20 },
                    new Part { PartCode = "FIL-001", PartName = "Lọc nhớt Toyota", UnitPrice = 120000, StockQuantity = 50 }
                );
            }

            // 3. Tạo 1 Admin mẫu để đăng nhập
            if (!context.Users.Any())
            {
                context.Users.Add(new User { Username = "admin", PasswordHash = "admin123", FullName = "Quản trị viên", Role = "Admin" });
            }

            context.SaveChanges();
        }
    }
}