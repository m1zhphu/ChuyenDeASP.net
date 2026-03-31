using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceItemService _serviceItemService;

        public ServicesController(IServiceItemService serviceItemService)
        {
            _serviceItemService = serviceItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _serviceItemService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var service = await _serviceItemService.GetByIdAsync(id);
            if (service == null) return NotFound(new { message = "Không tìm thấy dịch vụ." });
            return Ok(service);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequestDTO request)
        {
            var createdService = await _serviceItemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ServiceRequestDTO request)
        {
            var success = await _serviceItemService.UpdateAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy dịch vụ để cập nhật." });
            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _serviceItemService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy dịch vụ để xóa." });
            return Ok(new { message = "Xóa thành công." });
        }
    }
}