using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            var successProperty = result.GetType().GetProperty("success");
            bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

            if (!isSuccess) return Unauthorized(result); // Trả về mã lỗi 401 nếu sai pass

            return Ok(result);
        }
    }
}