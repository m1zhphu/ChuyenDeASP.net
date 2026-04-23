using Microsoft.AspNetCore.Mvc;
using SmartGarage.DTOs;
using SmartGarage.Interface;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairOrderController : ControllerBase
    {
        private readonly IRepairOrderService _orderService;

        public RepairOrderController(IRepairOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateRepairOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return Ok(result);
        }

        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetOrderDetails(string orderCode)
        {
            var order = await _orderService.GetOrderDetailsAsync(orderCode); // Đã sửa tên biến ở đây
            if (order == null) return NotFound(new { message = "Không tìm thấy phiếu sửa chữa." });
            return Ok(order);
        }
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PaymentRequestDTO request)
        {
            var result = await _orderService.ProcessPaymentAsync(request);
            return Ok(result);
        }
        // THÊM HÀM NÀY ĐỂ LẤY DANH SÁCH LỆNH SỬA CHỮA
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                // Gọi hàm lấy danh sách từ Service. 
                // (Lưu ý: Tên hàm GetAllAsync có thể khác tùy vào cách bạn đặt trong IRepairOrderService)
                var orders = await _orderService.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách: " + ex.Message });
            }
        }
        [HttpGet("history/{licensePlate}")]
        public async Task<IActionResult> GetVehicleHistory(string licensePlate)
        {
            try
            {
                // Gọi sang Service để lấy lịch sử
                var history = await _orderService.GetHistoryByLicensePlateAsync(licensePlate);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy lịch sử: " + ex.Message });
            }
        }
    }
}