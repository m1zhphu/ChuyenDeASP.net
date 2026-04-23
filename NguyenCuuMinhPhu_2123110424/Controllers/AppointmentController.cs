using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: api/Appointment
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _appointmentService.GetPagedAsync(page, pageSize, status, fromDate, toDate);
            return Ok(result);
        }

        // POST: api/Appointment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _appointmentService.CreateAsync(request);

            var successProperty = result.GetType().GetProperty("success");
            bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

            if (!isSuccess) return BadRequest(result);
            return Ok(result);
        }

        // PUT: api/Appointment/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AppointmentStatusUpdateDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _appointmentService.UpdateStatusAsync(id, request.Status);
            if (!success) return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }

        [HttpPost("public-book")]
        public async Task<IActionResult> BookOnline([FromBody] PublicBookingRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _appointmentService.BookOnlineAsync(request);
                var successProperty = result.GetType().GetProperty("success");
                bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

                if (!isSuccess) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}