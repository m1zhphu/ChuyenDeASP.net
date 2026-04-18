using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR; // THÊM DÒNG NÀY LÊN ĐẦU FILE
using SmartGarage.Hubs;

namespace SmartGarage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập để lấy Token
    public class TechnicianController : ControllerBase
    {
        private readonly GarageDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TechnicianController(GarageDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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
                .Select(os => new
                {
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
            // Dùng Include để lấy được tên Dịch vụ và Biển số xe để làm câu thông báo cho hay
            var task = await _context.RepairOrderServiceDetails
                .Include(os => os.Service)
                .Include(os => os.RepairOrder).ThenInclude(ro => ro.Vehicle)
                .FirstOrDefaultAsync(os => os.Id == taskId);

            if (task == null) return NotFound(new { message = "Không tìm thấy công việc này." });

            task.Status = request.Status;
            await _context.SaveChangesAsync();

            // === PHẦN MỚI: PHÁT THÔNG BÁO REAL-TIME ===
            if (request.Status == "Completed")
            {
                var licensePlate = task.RepairOrder?.Vehicle?.LicensePlate ?? "Không rõ biển số";
                var serviceName = task.Service?.ServiceName ?? "Dịch vụ";

                // Gửi thông báo đến TẤT CẢ các Client đang kết nối với sự kiện tên là "ReceiveNotification"
                await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                    $"🔔 Xe {licensePlate} đã hoàn thành hạng mục: {serviceName}. Vui lòng kiểm tra!");
            }

            return Ok(new { success = true, message = "Cập nhật thành công." });
        }
        // 3. LẤY LỊCH SỬ CÔNG VIỆC ĐÃ HOÀN THÀNH
        [HttpGet("history")]
        public async Task<IActionResult> GetTaskHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null) return Unauthorized(new { message = "Không tìm thấy token." });

            var userId = Guid.Parse(userIdClaim.Value);
            var role = roleClaim?.Value;

            var query = _context.RepairOrderServiceDetails
                .Include(os => os.Service)
                .Include(os => os.RepairOrder).ThenInclude(ro => ro.Vehicle)
                .Where(os => os.Status == "Completed");

            // Nếu không phải Admin thì chỉ lấy lịch sử của chính mình
            if (role != "Admin" && role != "ADMIN")
            {
                query = query.Where(os => os.MechanicId == userId);
            }

            var history = await query
                .OrderByDescending(os => os.RepairOrder.CreatedAt) // Mới nhất lên đầu
                .Select(os => new
                {
                    taskId = os.Id,
                    orderCode = os.RepairOrder.OrderCode,
                    licensePlate = os.RepairOrder.Vehicle.LicensePlate,
                    serviceName = os.Service.ServiceName,
                    status = os.Status,
                    mechanicName = _context.Users.Where(u => u.Id == os.MechanicId).Select(u => u.FullName).FirstOrDefault() ?? "Chưa rõ"
                })
                .ToListAsync();

            return Ok(history);
        }
        [HttpGet("scheduler-events")]
        public async Task<IActionResult> GetSchedulerEvents()
        {
            // Lấy dữ liệu và thực hiện xử lý ngày tháng sau khi đã ToList để tránh lỗi ép kiểu của Postgres
            var data = await _context.RepairOrderServiceDetails
                .Include(os => os.Service)
                .Include(os => os.RepairOrder).ThenInclude(ro => ro.Vehicle)
                .Include(os => os.Mechanic)
                .ToListAsync();

            var events = data.Select(os => new {
                id = os.Id,
                title = $"{os.Service.ServiceName} ({os.RepairOrder.Vehicle.LicensePlate})",
                // Ép kiểu tất cả về ToUniversalTime() để đồng bộ với PostgreSQL
                start = (os.ActualStartTime ?? os.RepairOrder.CreatedAt).ToUniversalTime(),
                end = (os.ActualEndTime ?? DateTime.UtcNow).ToUniversalTime(),
                resourceId = os.MechanicId,
                mechanicName = os.Mechanic != null ? os.Mechanic.FullName : "Chưa phân công",
                status = os.Status
            }).ToList();

            return Ok(events);
        }
        [HttpPut("update-event-time")]
        public async Task<IActionResult> UpdateEventTime([FromBody] UpdateEventTimeRequest request)
        {
            var task = await _context.RepairOrderServiceDetails.FindAsync(request.TaskId);
            if (task == null) return NotFound();

            // Cập nhật lại thời gian bắt đầu dự kiến (hoặc thực tế)
            task.ActualStartTime = request.NewStart.ToUniversalTime();
            // Giả sử mỗi việc kéo dài 2 tiếng nếu bạn chưa có cột thời gian kết thúc
            task.ActualEndTime = request.NewStart.AddHours(2).ToUniversalTime();

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        [HttpPut("reassign-mechanic")]
        public async Task<IActionResult> ReassignMechanic([FromBody] ReassignRequest request)
        {
            var task = await _context.RepairOrderServiceDetails.FindAsync(request.TaskId);
            if (task == null) return NotFound();

            // 1. Đổi người thợ mới
            task.MechanicId = request.NewMechanicId;

            // 2. ÉP BUỘC TRẠNG THÁI VỀ "Pending" (Chờ xử lý)
            // Việc này giúp xe biến mất khỏi "Lịch sử" và hiện lại trong "Công việc thợ" của thợ mới
            task.Status = "Pending";

            // 3. Xóa luôn ngày hoàn thành (nếu có) để lịch vẽ lại cho đúng
            task.ActualEndTime = null;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Thêm API lấy danh sách thợ để hiện lên dropdown ở Frontend
        [HttpGet("list-mechanics")]
        public async Task<IActionResult> GetMechanics()
        {
            var mechanics = await _context.Users
                .Where(u => u.Role == "Technician" || u.Role == "Admin")
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();
            return Ok(mechanics);
        }

        public class ReassignRequest
        {
            public Guid TaskId { get; set; }
            public Guid NewMechanicId { get; set; }
        }
        public class UpdateEventTimeRequest
        {
            public Guid TaskId { get; set; }
            public DateTime NewStart { get; set; }
        }

        // Class phụ để nhận dữ liệu Update
        public class UpdateTaskRequest
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}