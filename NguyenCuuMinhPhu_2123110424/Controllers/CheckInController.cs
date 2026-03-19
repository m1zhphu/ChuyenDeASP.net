using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Services;
using SmartGarage.Interface;
namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        // Bơm Service vào thay vì bơm trực tiếp DbContext
        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanLicensePlate([FromBody] ScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicensePlate))
            {
                return BadRequest(new { message = "Vui lòng cung cấp biển số xe." });
            }

            // Đẩy logic xuống Service xử lý
            var result = await _checkInService.ProcessScanAsync(request.LicensePlate);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterNewCustomer([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicensePlate) || string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "Biển số xe và Họ tên là bắt buộc." });
            }

            // Đẩy logic xuống Service xử lý
            var isSuccess = await _checkInService.RegisterCustomerAsync(request);

            if (isSuccess)
            {
                return Ok(new { message = "Đã lưu hồ sơ khách hàng và xe thành công!" });
            }

            return StatusCode(500, new { message = "Có lỗi xảy ra khi lưu vào hệ thống." });
        }
    }
}