using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using System.Security.Claims;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập để lấy Token
    public class TechnicianController : ControllerBase
    {
        private readonly GarageDbContext _context;

        public TechnicianController(GarageDbContext context)
        {
            _context = context;
        }

        // 1. THỢ MÁY LẤY DANH SÁCH VIỆC ĐƯỢC GIAO
        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role); // Lấy Role để kiểm tra quyền

            if (userIdClaim == null) return Unauthorized(new { message = "Không tìm thấy token." });

            var userId = Guid.Parse(userIdClaim.Value);
            var role = roleClaim?.Value;

            var query = _context.RepairOrderServiceDetails
                .Include(os => os.Service)
                .Include(os => os.RepairOrder).ThenInclude(ro => ro.Vehicle)
                .Where(os => os.Status != "Completed");

            // NẾU KHÔNG PHẢI ADMIN THÌ CHỈ LẤY VIỆC CỦA CHÍNH MÌNH
            if (role != "Admin" && role != "ADMIN")
            {
                query = query.Where(os => os.MechanicId == userId);
            }

            var tasks = await query
                .Select(os => new {
                    taskId = os.Id,
                    orderCode = os.RepairOrder.OrderCode,
                    licensePlate = os.RepairOrder.Vehicle.LicensePlate,
                    serviceName = os.Service.ServiceName,
                    status = os.Status,
                    // Thêm tên người thợ để Admin nhìn vào biết việc này của ai
                    mechanicName = _context.Users.Where(u => u.Id == os.MechanicId).Select(u => u.FullName).FirstOrDefault() ?? "Chưa phân công"
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // 2. THỢ MÁY CẬP NHẬT TRẠNG THÁI (Bắt đầu làm / Đã xong)
        [HttpPut("tasks/{taskId}")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromBody] UpdateTaskRequest request)
        {
            var task = await _context.RepairOrderServiceDetails.FindAsync(taskId);
            if (task == null) return NotFound(new { message = "Không tìm thấy công việc này." });

            task.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thành công." });
        }
    }

    // Class phụ để nhận dữ liệu Update
    public class UpdateTaskRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}