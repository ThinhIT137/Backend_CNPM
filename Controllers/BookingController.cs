using backend.DTO;
using backend.Hubs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được đặt vé/phòng
    [ApiController]
    [Route("/api/[controller]")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly CnpmContext _context;

        public BookingController(IBookingService bookingService, CnpmContext context, IHubContext<NotificationHub> hubContext)
        {
            _bookingService = bookingService;
            _context = context;
            _hubContext = hubContext;
        }

        // ===============================================
        // 1. KHÁCH HÀNG: TẠO ĐƠN ĐẶT CHỖ
        // ===============================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            await _bookingService.CreateBookingAsync(userId, req);

            return Ok(new { success = true, message = "Đặt chỗ thành công! Đang chờ xác nhận." });
        }

        // ===============================================
        // 2. OWNER: XEM THỐNG KÊ DOANH THU & ĐƠN HÀNG
        // ===============================================
        [Authorize(Roles = "Owner, Admin, Hotel, Tour")]
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetOwnerDashboardStats()
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Bước 1: Lấy ID các Phòng và Chuyến đi mà Owner này SỞ HỮU
            var myRoomIds = await _context.HotelRooms.Where(r => r.Hotel!.Created_By_UserId == ownerId).Select(r => r.Id).ToListAsync();
            var myDepartureIds = await _context.TourDepartures.Where(d => d.Tour!.Created_By_UserId == ownerId).Select(d => d.Id).ToListAsync();

            // Bước 2: Kéo toàn bộ Chi tiết đơn hàng có chứa các món trên
            var myDetails = await _context.BookingDetails
                .Include(bd => bd.Booking)
                .Where(bd => (bd.HotelRoomId.HasValue && myRoomIds.Contains(bd.HotelRoomId.Value)) ||
                             (bd.TourDepartureId.HasValue && myDepartureIds.Contains(bd.TourDepartureId.Value)))
                .ToListAsync();

            // Tính tổng số đơn (Lọc distinct để tránh đếm trùng nếu 1 đơn đặt 2 phòng)
            var totalBookings = myDetails.Select(bd => bd.BookingId).Distinct().Count();

            // Tính tổng tiền (Chỉ tính những đơn đã thanh toán hoặc đã hoàn thành)
            var revenueDetails = myDetails.Where(bd => bd.Booking!.PaymentStatus == "Paid" || bd.Booking.BookingStatus == "Completed").ToList();
            var totalRevenue = revenueDetails.Sum(bd => bd.UnitPrice);

            // Tính doanh thu theo từng tháng trong năm nay
            var currentYear = DateTime.Now.Year;
            var revenueByMonth = revenueDetails
                .Where(bd => bd.Booking!.CreatedAt.Year == currentYear)
                .GroupBy(bd => bd.Booking!.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(bd => bd.UnitPrice)
                }).ToList();

            // Mẹo Frontend: Trả đủ 12 tháng (kể cả tháng 0 đồng) để FE vẽ biểu đồ cho dễ
            var fullYearRevenue = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Revenue = revenueByMonth.FirstOrDefault(r => r.Month == m)?.Revenue ?? 0
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalBookings,
                    totalRevenue,
                    revenueByMonth = fullYearRevenue
                }
            });
        }

        // ===============================================
        // 3. OWNER: LẤY DANH SÁCH ĐƠN KHÁCH VỪA ĐẶT
        // ===============================================
        [Authorize(Roles = "Owner, Admin, Hotel, Tour")]
        [HttpGet("received-bookings")]
        public async Task<IActionResult> GetReceivedBookings()
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var myRoomIds = await _context.HotelRooms.Where(r => r.Hotel!.Created_By_UserId == ownerId).Select(r => r.Id).ToListAsync();
            var myDepartureIds = await _context.TourDepartures.Where(d => d.Tour!.Created_By_UserId == ownerId).Select(d => d.Id).ToListAsync();

            // Kéo toàn bộ Booking có chứa hàng của Owner này
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.HotelRoom).ThenInclude(hr => hr!.Hotel)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TourDeparture).ThenInclude(td => td!.Tour)
                .Where(b => b.BookingDetails.Any(bd =>
                    (bd.HotelRoomId.HasValue && myRoomIds.Contains(bd.HotelRoomId.Value)) ||
                    (bd.TourDepartureId.HasValue && myDepartureIds.Contains(bd.TourDepartureId.Value))
                ))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Map sang DTO sạch sẽ (Chống lỗi Circular Reference của JSON)
            var result = bookings.Select(b => new
            {
                id = b.Id,
                customerName = b.ContactName,
                customerPhone = b.ContactPhone,
                bookingType = b.BookingType,
                totalAmount = b.TotalAmount,
                paymentStatus = b.PaymentStatus,
                bookingStatus = b.BookingStatus, // Pending, Confirmed... -> Để FE hiện nút Duyệt
                createdAt = b.CreatedAt,

                // Chỉ hiện những chi tiết món hàng do Owner này bán (Phòng hờ khách đặt chung 1 hóa đơn nhưng nhiều Khách sạn khác nhau)
                details = b.BookingDetails
                    .Where(bd => (bd.HotelRoomId.HasValue && myRoomIds.Contains(bd.HotelRoomId.Value)) ||
                                 (bd.TourDepartureId.HasValue && myDepartureIds.Contains(bd.TourDepartureId.Value)))
                    .Select(bd => new
                    {
                        detailId = bd.Id,
                        unitPrice = bd.UnitPrice,
                        productName = bd.HotelRoomId.HasValue
                            ? $"{bd.HotelRoom?.Hotel?.Name} - {bd.HotelRoom?.RoomName}"
                            : bd.TourDeparture?.Tour?.Name,
                        info = bd.HotelRoomId.HasValue
                            ? $"Tầng {bd.HotelRoom?.Floor} | {bd.HotelRoom?.RoomType}"
                            : (bd.IsPrivateTour ? "Bao nguyên xe" : $"Ghế: {bd.SeatNumber}")
                    }).ToList()
            }).ToList();

            return Ok(new { success = true, data = result });
        }

        // ===============================================
        // 4. OWNER: DUYỆT HOẶC HỦY ĐƠN HÀNG (KỂ CẢ KHI ĐÃ ĐẶT THÀNH CÔNG)
        // ===============================================
        [Authorize(Roles = "Owner, Admin, Hotel, Tour")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusReq req)
        {
            // Tìm đơn hàng kèm thông tin khách
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound(new { success = false, message = "Không tìm thấy đơn đặt" });

            // Kiểm tra tính hợp lệ của trạng thái mới
            var validStatus = new[] { "Confirmed", "Cancelled", "Completed" };
            if (!validStatus.Contains(req.Status))
                return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });

            // NẾU đơn hàng đã Completed thì không cho phép đổi trạng thái nữa (Bảo vệ dữ liệu doanh thu)
            if (booking.BookingStatus == "Completed")
                return BadRequest(new { success = false, message = "Đơn hàng đã hoàn tất, không thể thay đổi trạng thái." });

            // Thực hiện cập nhật
            booking.BookingStatus = req.Status;

            // Tạo nội dung thông báo linh hoạt theo trạng thái
            string notifTitle = "";
            string notifContent = "";

            switch (req.Status)
            {
                case "Confirmed":
                    notifTitle = "✅ Đơn hàng đã được duyệt";
                    notifContent = $"Tuyệt vời! Đơn đặt chỗ #{id} của bạn đã được chủ cơ sở xác nhận thành công.";
                    break;
                case "Cancelled":
                    notifTitle = "❌ Đơn hàng đã bị hủy bởi chủ";
                    notifContent = $"Chúng tôi rất tiếc, đơn đặt chỗ #{id} của bạn đã bị hủy bởi chủ cơ sở (do sự cố hoặc yêu cầu của bạn).";
                    break;
                case "Completed":
                    notifTitle = "🎉 Dịch vụ đã hoàn tất";
                    notifContent = $"Cảm ơn bạn đã sử dụng dịch vụ. Đơn hàng #{id} đã được đánh dấu là hoàn tất.";
                    break;
            }

            var notif = new Notification
            {
                UserId = booking.UserId, // Gửi cho Khách hàng
                Title = notifTitle,
                Content = notifContent,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Bắn SignalR báo cho Khách biết ngay lập tức
            await _hubContext.Clients.User(booking.UserId.ToString()).SendAsync("ReceiveNotification", new
            {
                id = notif.Id,
                title = notif.Title,
                content = notif.Content,
                createdAt = notif.CreatedAt,
                isRead = false
            });

            return Ok(new { success = true, message = $"Đã cập nhật đơn hàng #{id} thành {req.Status}" });
        }

        // ===============================================
        // KHÁCH HÀNG: LẤY LỊCH SỬ ĐƠN ĐÃ ĐẶT (MY BOOKINGS)
        // ===============================================
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.BookingDetails).ThenInclude(bd => bd.HotelRoom).ThenInclude(hr => hr!.Hotel)
                    .Include(b => b.BookingDetails).ThenInclude(bd => bd.TourDeparture).ThenInclude(td => td!.Tour)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .AsNoTracking() // 🔴 NHẸ RAM
                    .AsSplitQuery() // 🔴 CỨU TINH CHỐNG LỖI 500 (Chia nhỏ SQL)
                    .ToListAsync();

                var result = bookings.Select(b => new
                {
                    id = b.Id,
                    customerName = b.ContactName,
                    customerPhone = b.ContactPhone,
                    contactAddress = b.ContactAddress, // 🔴 BẾ THÊM ÔNG THẦN NÀY VÀO ĐÂY NHÉ SẾP
                    bookingType = b.BookingType,
                    totalAmount = b.TotalAmount,
                    paymentStatus = b.PaymentStatus,
                    bookingStatus = b.BookingStatus,
                    createdAt = b.CreatedAt,

                    // 🔴 Bọc Null kỹ càng, thách DB có thiếu dữ liệu cũng không sập được
                    details = (b.BookingDetails ?? Enumerable.Empty<Booking_Detail>()).Select(bd => new
                    {
                        detailId = bd.Id,
                        unitPrice = bd.UnitPrice,
                        productName = bd.HotelRoomId.HasValue
                            ? $"{bd.HotelRoom?.Hotel?.Name ?? "Khách sạn"} - {bd.HotelRoom?.RoomName ?? "Phòng"}"
                            : bd.TourDeparture?.Tour?.Name ?? "Tour",
                        info = bd.HotelRoomId.HasValue
                            ? $"Tầng {bd.HotelRoom?.Floor} | {bd.HotelRoom?.RoomType}"
                            : (bd.IsPrivateTour ? "Bao nguyên xe" : $"Ghế: {bd.SeatNumber ?? "N/A"}")
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // In thẳng lỗi thật ra Terminal để bắt bệnh nếu còn xui xẻo
                Console.WriteLine("LỖI GET MY BOOKINGS: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Lỗi nội bộ: " + ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            // 1. Lấy và kiểm tra ID người dùng (Khách) đang đăng nhập
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            // 2. Tìm đơn hàng (Nhớ Include để lấy thông tin Owner)
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.HotelRoom).ThenInclude(hr => hr!.Hotel)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TourDeparture).ThenInclude(td => td!.Tour)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn đặt chỗ hoặc bạn không có quyền hủy." });

            // 3. Kiểm tra trạng thái
            if (booking.BookingStatus != "Pending")
                return BadRequest(new { success = false, message = "Bạn chỉ có thể hủy đơn khi đang ở trạng thái 'Chờ duyệt'." });

            // 4. Cập nhật trạng thái thành Cancelled
            booking.BookingStatus = "Cancelled";

            // 5. Tạo thông báo cho KHÁCH HÀNG
            var customerNotif = new Notification
            {
                UserId = userId,
                Title = "❌ Hủy đơn thành công",
                Content = $"Bạn đã tự hủy đơn đặt chỗ #{id} thành công.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(customerNotif);

            // 6. Tìm danh sách OWNER liên quan đến đơn hàng này và tạo thông báo
            var ownerIds = booking.BookingDetails
                .Select(bd => bd.HotelRoom?.Hotel?.Created_By_UserId ?? bd.TourDeparture?.Tour?.Created_By_UserId)
                .Where(ownerId => ownerId.HasValue)
                .Select(ownerId => ownerId!.Value)
                .Distinct() // Lọc trùng lặp (ví dụ mua 2 phòng của cùng 1 chủ thì chỉ báo 1 lần)
                .ToList();

            var ownerNotifs = new List<Notification>();
            foreach (var ownerId in ownerIds)
            {
                var ownerNotif = new Notification
                {
                    UserId = ownerId,
                    Title = "⚠️ Đơn đặt chỗ bị hủy",
                    Content = $"Khách hàng {booking.ContactName} đã tự hủy đơn đặt chỗ #{id}.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(ownerNotif);
                ownerNotifs.Add(ownerNotif);
            }

            // Ghi tất cả thay đổi (Trạng thái đơn + Tất cả Noti) vào Database 1 lần duy nhất
            await _context.SaveChangesAsync();

            // 7. Bắn SignalR Real-time cho KHÁCH HÀNG
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                id = customerNotif.Id,
                title = customerNotif.Title,
                content = customerNotif.Content,
                createdAt = customerNotif.CreatedAt,
                isRead = false
            });

            // 8. Bắn SignalR Real-time cho các CHỦ CƠ SỞ (OWNER)
            foreach (var notif in ownerNotifs)
            {
                await _hubContext.Clients.User(notif.UserId.ToString()).SendAsync("ReceiveNotification", new
                {
                    id = notif.Id,
                    title = notif.Title,
                    content = notif.Content,
                    createdAt = notif.CreatedAt,
                    isRead = false
                });
            }

            return Ok(new { success = true, message = "Đã hủy đơn đặt chỗ thành công." });
        }
    }
}
