using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng." });
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerRequestDTO request)
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerRequestDTO request)
        {
            var success = await _customerService.UpdateCustomerAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy khách hàng để cập nhật." });
            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy khách hàng để xóa." });
            return Ok(new { message = "Xóa thành công." });
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var customers = await _customerService.SearchCustomersAsync(keyword);
            return Ok(customers);
        }
    }
}