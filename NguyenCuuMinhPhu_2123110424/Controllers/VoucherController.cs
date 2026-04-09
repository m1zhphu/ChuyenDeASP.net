using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // GET: api/Voucher
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _voucherService.GetPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        // POST: api/Voucher
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _voucherService.CreateAsync(request);

            var successProperty = result.GetType().GetProperty("success");
            bool isSuccess = (bool)(successProperty?.GetValue(result) ?? false);

            if (!isSuccess) return BadRequest(result);
            return Ok(result);
        }

        // GET: api/Voucher/validate/TRIAN2026
        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidateVoucher(string code)
        {
            var result = await _voucherService.ValidateVoucherAsync(code);

            var isValidProperty = result.GetType().GetProperty("isValid");
            bool isValid = (bool)(isValidProperty?.GetValue(result) ?? false);

            if (!isValid) return BadRequest(result);
            return Ok(result);
        }
    }
}