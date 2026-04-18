using Microsoft.AspNetCore.SignalR;

namespace SmartGarage.Hubs
{
    // Kế thừa từ Hub của SignalR
    public class NotificationHub : Hub
    {
        // Hiện tại Server chỉ có nhiệm vụ "đẩy" thông báo xuống Client, 
        // nên class này chúng ta để trống là đủ! Rất gọn gàng.
    }
}