using Microsoft.AspNetCore.Mvc;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [HttpGet("{licensePlate}")]
        public async Task<IActionResult> CheckIn(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return BadRequest(new { message = "Vui lòng nhập biển số xe." });

            var result = await _checkInService.ProcessCheckInAsync(licensePlate);
            return Ok(result);
        }
        [HttpPost("quick-onboard")]
        public async Task<IActionResult> QuickOnboard([FromBody] SmartGarage.DTOs.QuickOnboardRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.LicensePlate) || string.IsNullOrWhiteSpace(request.CustomerName))
                return BadRequest(new { message = "Biển số và Tên khách hàng là bắt buộc." });

            try
            {
                var result = await _checkInService.QuickOnboardAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo hồ sơ.", error = ex.Message });
            }
        }
    }
}