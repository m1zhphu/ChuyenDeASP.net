using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET: api/Supplier?page=1&pageSize=10&search=honda
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _supplierService.GetPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        // GET: api/Supplier/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null) return NotFound(new { message = "Không tìm thấy nhà cung cấp." });
            return Ok(supplier);
        }

        // POST: api/Supplier
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _supplierService.CreateAsync(request);
            return Ok(result);
        }

        // PUT: api/Supplier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SupplierRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _supplierService.UpdateAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy nhà cung cấp để cập nhật." });
            return Ok(new { message = "Cập nhật thành công." });
        }

        // DELETE: api/Supplier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _supplierService.DeleteAsync(id);

            // Ép kiểu dynamic để lấy thuộc tính success (vì mình return object ẩn danh từ Service)
            var dict = result as System.Dynamic.ExpandoObject;
            var successProperty = result.GetType().GetProperty("success");
            bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

            if (!isSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}