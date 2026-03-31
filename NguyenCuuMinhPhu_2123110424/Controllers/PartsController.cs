using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _partService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var part = await _partService.GetByIdAsync(id);
            if (part == null) return NotFound(new { message = "Không tìm thấy phụ tùng." });
            return Ok(part);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartRequestDTO request)
        {
            var createdPart = await _partService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdPart.Id }, createdPart);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PartRequestDTO request)
        {
            var success = await _partService.UpdateAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy phụ tùng để cập nhật." });
            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _partService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy phụ tùng để xóa." });
            return Ok(new { message = "Xóa thành công." });
        }
    }
}