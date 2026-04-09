using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly GarageDbContext _context;

        public InventoryService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateReceiptAsync(CreateReceiptRequestDTO request)
        {
            // 1. Kiểm tra Nhà cung cấp có tồn tại không
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
            if (!supplierExists) return new { success = false, message = "Nhà cung cấp không tồn tại." };

            // 2. Khởi tạo Phiếu Nhập
            var receipt = new InventoryReceipt
            {
                ReceiptCode = request.ReceiptCode,
                SupplierId = request.SupplierId,
                UserId = request.UserId,
                Note = request.Note,
                TotalAmount = 0
            };

            decimal totalAmount = 0;

            // 3. Xử lý từng mặt hàng nhập vào
            foreach (var item in request.Details)
            {
                var part = await _context.Parts.FindAsync(item.PartId);
                if (part == null)
                    return new { success = false, message = $"Không tìm thấy phụ tùng với mã ID: {item.PartId}" };

                // Tạo chi tiết phiếu nhập
                receipt.ReceiptDetails.Add(new InventoryReceiptDetail
                {
                    PartId = part.Id,
                    Quantity = item.Quantity,
                    ImportPrice = item.ImportPrice
                });

                // CỘNG DỒN TỒN KHO CHO GARA (Đây là dòng code quan trọng nhất)
                part.StockQuantity += item.Quantity;

                // Cộng tiền vào tổng bill
                totalAmount += (item.Quantity * item.ImportPrice);
            }

            receipt.TotalAmount = totalAmount;

            // 4. Lưu toàn bộ vào Database (Entity Framework sẽ tự bọc trong 1 Transaction)
            _context.InventoryReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            return new { success = true, receiptCode = receipt.ReceiptCode, totalAmount = receipt.TotalAmount };
        }

        public async Task<object> GetPagedReceiptsAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.InventoryReceipts
                .Include(r => r.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.ReceiptCode.Contains(search) || r.Supplier.SupplierName.Contains(search));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReceiptResponseDTO
                {
                    Id = r.Id,
                    ReceiptCode = r.ReceiptCode,
                    SupplierName = r.Supplier.SupplierName,
                    TotalAmount = r.TotalAmount,
                    CreatedAt = r.CreatedAt,
                    Note = r.Note
                }).ToListAsync();

            return new { TotalRecords = totalRecords, TotalPages = totalPages, CurrentPage = page, Data = data };
        }

        public async Task<ReceiptResponseDTO?> GetReceiptDetailsAsync(Guid id)
        {
            var receipt = await _context.InventoryReceipts
                .Include(r => r.Supplier)
                .Include(r => r.ReceiptDetails)
                    .ThenInclude(d => d.Part)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return null;

            return new ReceiptResponseDTO
            {
                Id = receipt.Id,
                ReceiptCode = receipt.ReceiptCode,
                SupplierName = receipt.Supplier.SupplierName,
                TotalAmount = receipt.TotalAmount,
                CreatedAt = receipt.CreatedAt,
                Note = receipt.Note,
                Items = receipt.ReceiptDetails.Select(d => new ReceiptItemResponseDTO
                {
                    PartName = d.Part.PartName,
                    Quantity = d.Quantity,
                    ImportPrice = d.ImportPrice
                }).ToList()
            };
        }
    }
}