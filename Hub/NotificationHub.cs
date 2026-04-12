using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace backend.Hubs
{
    [Authorize] // Chỉ cho phép người đã đăng nhập kết nối
    public class NotificationHub : Hub
    {
        // Khi user kết nối, SignalR tự động map UserId từ ClaimTypes.NameIdentifier vào Hub
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}