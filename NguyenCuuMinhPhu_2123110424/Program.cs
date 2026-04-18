using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using SmartGarage.Data;
using SmartGarage.Interface;
using SmartGarage.Services;
using System.Text;

namespace NguyenCuuMinhPhu_2123110424
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==============================================================
            // 1. KẾT NỐI DATABASE
            // ==============================================================
            builder.Services.AddDbContext<GarageDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,            // Thử lại tối đa 5 lần
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Mỗi lần cách nhau tối đa 10 giây
                    errorCodesToAdd: null))      // Các mã lỗi bổ sung (nếu có)
            );

            // ==============================================================
            // 2. ĐĂNG KÝ DEPENDENCY INJECTION (DI)
            // ==============================================================
            builder.Services.AddScoped<ICheckInService, CheckInService>();
            builder.Services.AddScoped<IRepairOrderService, RepairOrderService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IPartService, PartService>();
            builder.Services.AddScoped<IServiceItemService, ServiceItemService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IVoucherService, VoucherService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<DashboardService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors();
            // ==============================================================
            // 3. CẤU HÌNH SWAGGER (Tích hợp nút Authorize nhập Token)
            // ==============================================================
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartGarage API v1", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập token JWT của bạn vào đây (Không cần gõ chữ Bearer ở trước):",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ==============================================================
            // 4. CẤU HÌNH BẢO MẬT JWT
            // ==============================================================
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            var app = builder.Build();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            // ==============================================================
            // 5. TỰ ĐỘNG TẠO BẢNG (MIGRATION) & NẠP DỮ LIỆU MẪU
            // ==============================================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<GarageDbContext>();

                    // ---> ĐÂY LÀ DÒNG THÊM VÀO ĐỂ TỰ ĐỘNG TẠO BẢNG TRÊN RENDER <---
                    context.Database.Migrate();

                    // Nạp dữ liệu mẫu
                    DbSeeder.Seed(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Có lỗi xảy ra trong quá trình tạo bảng hoặc nạp dữ liệu mẫu.");
                }
            }

            // ==============================================================
            // 6. PIPELINE MIDDLEWARE
            // ==============================================================
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartGarage API v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            // THỨ TỰ BẮT BUỘC: Authentication phải đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}