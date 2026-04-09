using Microsoft.EntityFrameworkCore;
using SmartGarage.Models;

namespace SmartGarage.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options) { }

        // ==========================================
        // 1. NHÓM DANH MỤC & NHÂN SỰ
        // ==========================================
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Part> Parts { get; set; }

        // ==========================================
        // 2. NHÓM GIAO DỊCH SỬA CHỮA (CỐT LÕI)
        // ==========================================
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<RepairOrderServiceDetail> RepairOrderServiceDetails { get; set; }
        public DbSet<RepairOrderPartDetail> RepairOrderPartDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // ==========================================
        // 3. NHÓM MỞ RỘNG NÂNG CAO (KHO, LỊCH HẸN, MARKETING)
        // ==========================================
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Ràng buộc: Biển số xe không bao giờ được trùng lặp trong Database
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // 2. Ràng buộc: Mã phiếu sửa chữa & Nhập kho phải là duy nhất
            modelBuilder.Entity<RepairOrder>()
                .HasIndex(r => r.OrderCode)
                .IsUnique();
            modelBuilder.Entity<InventoryReceipt>()
                .HasIndex(i => i.ReceiptCode)
                .IsUnique();

            // 3. Chống lỗi Cascade Delete (Xóa dây chuyền nguy hiểm)
            modelBuilder.Entity<RepairOrder>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryReceipt>()
                .HasOne(i => i.Supplier)
                .WithMany()
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============================================================
            // 4. FIX CẢNH BÁO DECIMAL: Tự động set kiểu decimal(18,2) cho toàn bộ các cột tiền
            // ==============================================================
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                        .SelectMany(t => t.GetProperties())
                        .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}