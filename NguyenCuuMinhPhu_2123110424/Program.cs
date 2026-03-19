using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.Services;   // <--- Khai báo chỗ chứa CheckInService
using SmartGarage.Interface;  // <--- Khai báo chỗ chứa ICheckInService

namespace NguyenCuuMinhPhu_2123110424
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Kết nối Database
            builder.Services.AddDbContext<GarageDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ==============================================================
            // 2. ĐĂNG KÝ DEPENDENCY INJECTION (DI) CHO SERVICE
            // ==============================================================
            builder.Services.AddScoped<ICheckInService, CheckInService>();
            builder.Services.AddScoped<IRepairOrderService, RepairOrderService>();
            // 3. Thêm Controller
            builder.Services.AddControllers();

            // Cấu hình Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<GarageDbContext>();
                    // Gọi hàm Seed từ file DbSeeder.cs mà bạn đã tạo ở bước trước
                    DbSeeder.Seed(context);
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu việc nạp dữ liệu thất bại
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Có lỗi xảy ra trong quá trình nạp dữ liệu mẫu.");
                }
            }
            // Hiển thị giao diện Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}