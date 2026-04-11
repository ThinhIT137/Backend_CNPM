using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly CnpmContext _context;

        public BookingController(IBookingService bookingService, CnpmContext context)
        {
            _bookingService = bookingService;
            _context = context;
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
        [Authorize(Roles = "Owner, Admin")]
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
        [Authorize(Roles = "Owner, Admin")]
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
        // 4. OWNER: DUYỆT ĐƠN HÀNG (XÁC NHẬN / TỪ CHỐI)
        // ===============================================
        [Authorize(Roles = "Owner, Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusReq req)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound(new { success = false, message = "Không tìm thấy đơn đặt" });

            var validStatus = new[] { "Confirmed", "Cancelled", "Completed" };
            if (!validStatus.Contains(req.Status))
                return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });

            booking.BookingStatus = req.Status;

            // Gửi chuông thông báo cho Khách hàng biết đơn đã được Owner duyệt
            _context.Notifications.Add(new Notification
            {
                UserId = booking.UserId,
                Title = "Cập nhật đơn hàng",
                Content = $"Đơn đặt chỗ #{id} của bạn đã được chuyển sang trạng thái: {req.Status}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"Đã cập nhật đơn hàng thành {req.Status}" });
        }
    }
}
