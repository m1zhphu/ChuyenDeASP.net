using Microsoft.EntityFrameworkCore;
using SmartGarage.Models;

namespace SmartGarage.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
        {
        }

        // 1. Nhóm Quản lý Truy cập & Nhân sự
        public DbSet<User> Users { get; set; }

        // 2. Nhóm Dữ liệu Khách hàng & Xe
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        // 3. Nhóm Danh mục (Master Data)
        public DbSet<Service> Services { get; set; }
        public DbSet<Part> Parts { get; set; }

        // 4. Nhóm Nghiệp vụ & Giao dịch
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<RepairOrderServiceDetail> RepairOrderServices { get; set; }
        public DbSet<RepairOrderPartDetail> RepairOrderParts { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Biển số xe là Unique (Không được trùng lặp)
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // Cấu hình định dạng tiền tệ (Decimal) để tránh cảnh báo từ SQL Server
            // Giúp số tiền chính xác đến 2 chữ số thập phân
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}