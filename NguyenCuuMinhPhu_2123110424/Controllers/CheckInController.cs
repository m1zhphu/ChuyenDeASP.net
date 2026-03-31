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
    }
}