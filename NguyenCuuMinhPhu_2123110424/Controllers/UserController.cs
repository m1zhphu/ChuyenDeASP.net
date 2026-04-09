using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using System.Security.Claims;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, string? search = null)
        {
            var result = await _userService.GetPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UserRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
        {
            var success = await _userService.UpdateStatusAsync(id, isActive);
            if (!success) return NotFound(new { message = "Không tìm thấy người dùng." });
            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var success = await _userService.ChangePasswordAsync(Guid.Parse(userIdClaim), request);
            if (!success) return BadRequest(new { message = "Mật khẩu cũ không chính xác." });

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
    }
}