using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // POST: api/Inventory/import (Tạo phiếu nhập kho và cộng tồn kho)
        [HttpPost("import")]
        public async Task<IActionResult> CreateReceipt([FromBody] CreateReceiptRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _inventoryService.CreateReceiptAsync(request);

            // Kiểm tra success
            var successProperty = result.GetType().GetProperty("success");
            bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

            if (!isSuccess) return BadRequest(result);
            return Ok(result);
        }

        // GET: api/Inventory?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetPagedReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _inventoryService.GetPagedReceiptsAsync(page, pageSize, search);
            return Ok(result);
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceiptDetails(Guid id)
        {
            var receipt = await _inventoryService.GetReceiptDetailsAsync(id);
            if (receipt == null) return NotFound(new { message = "Không tìm thấy phiếu nhập kho." });
            return Ok(receipt);
        }
    }
}