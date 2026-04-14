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
    [ApiController]
    [Route("/api/[controller]")]
    public class AdminController : Controller
    {
        private readonly CnpmContext _context;
        private readonly IUserService _userService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(CnpmContext context, IUserService userService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userService = userService;
            _hubContext = hubContext;
        }

        // ===============================================
        // 1. THỐNG KÊ DASHBOARD (ADMIN)
        // ===============================================
        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetSystemStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalHotels = await _context.Hotels.CountAsync();
            var totalTours = await _context.Tours.CountAsync();

            var pendingApprovals = await _context.Hotels.CountAsync(h => h.Status == "Pending")
                                 + await _context.Tours.CountAsync(t => t.Status == "Pending");

            var pendingReports = await _context.Reports.CountAsync(r => r.Status == "Pending");
            var totalTouristAreas = await _context.TouristAreas.CountAsync();
            var totalTouristPlaces = await _context.TouristPlaces.CountAsync();
            var activeAds = await _context.Advertisements
                        .CountAsync(a => a.IsActive && a.Start_date <= DateTime.Now && a.End_date >= DateTime.Now);
            var feedBack = await _context.Feedbacks.CountAsync(f => f.Status == "Pending");

            // Đã xóa totalRevenue ở đây
            return Ok(new
            {
                success = true,
                data = new
                {
                    totalUsers,
                    totalHotels,
                    totalTours,
                    pendingApprovals,
                    pendingReports,
                    totalTouristAreas,
                    totalTouristPlaces,
                    activeAds,
                    feedBack
                }
            });
        }

        // ===============================================
        // 2. QUẢN LÝ QUẢNG CÁO (ADMIN)
        // ===============================================

        [Authorize(Roles = "Admin")]
        [HttpPost("ads")]
        public async Task<IActionResult> CreateAd([FromBody] AdRequest req)
        {
            var ad = new Advertisement
            {
                Title = req.Title,
                Description = req.Description,
                Position = req.Position,
                Url = req.Url,
                Name = req.Name,
                Phone = req.Phone,
                Start_date = DateTime.Now,
                End_date = DateTime.Now.AddDays(30), // Mặc định chạy 30 ngày (sếp có thể tùy chỉnh)
                IsActive = true
            };
            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Tạo quảng cáo thành công" });
        }

        // 🔴 THÊM: LẤY TẤT CẢ QUẢNG CÁO CHO ADMIN QUẢN LÝ
        [Authorize(Roles = "Admin")]
        [HttpGet("ads/all")]
        public async Task<IActionResult> GetAllAds([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
        {
            var query = _context.Advertisements.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Name.Contains(keyword) || a.Phone.Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.Start_date)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    items = items,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    currentPage = page
                }
            });
        }

        // 🔴 THÊM: CẬP NHẬT QUẢNG CÁO
        [Authorize(Roles = "Admin")]
        [HttpPut("ads/{id:int}")]
        public async Task<IActionResult> UpdateAd(int id, [FromBody] AdRequest req)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return NotFound(new { success = false, message = "Không tìm thấy quảng cáo" });

            ad.Title = req.Title;
            ad.Description = req.Description;
            ad.Position = req.Position;
            ad.Url = req.Url;
            ad.Name = req.Name;
            ad.Phone = req.Phone;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật quảng cáo thành công" });
        }

        // 🔴 THÊM: XÓA QUẢNG CÁO
        [Authorize(Roles = "Admin")]
        [HttpDelete("ads/{id:int}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return NotFound(new { success = false, message = "Không tìm thấy quảng cáo" });

            _context.Advertisements.Remove(ad);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Xóa quảng cáo thành công" });
        }

        // 🔴 THÊM: BẬT/TẮT QUẢNG CÁO NHANH
        [Authorize(Roles = "Admin")]
        [HttpPut("ads/{id:int}/toggle")]
        public async Task<IActionResult> ToggleAdStatus(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return NotFound(new { success = false, message = "Không tìm thấy quảng cáo" });

            ad.IsActive = !ad.IsActive; // Đảo ngược trạng thái
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = ad.IsActive ? "Đã BẬT quảng cáo" : "Đã TẮT quảng cáo" });
        }

        [AllowAnonymous] // App gọi ra để hiển thị quảng cáo
        [HttpGet("ads/active")]
        public async Task<IActionResult> GetActiveAds([FromQuery] string position = "Home")
        {
            var ads = await _context.Advertisements
                .Where(a => a.IsActive && a.Start_date <= DateTime.Now && a.End_date >= DateTime.Now && a.Position == position)
                .ToListAsync();
            return Ok(new { success = true, data = ads });
        }

        // ===============================================
        // 3. HỆ THỐNG REPORT (BÁO CÁO VI PHẠM)
        // ===============================================
        [Authorize(Roles = "Admin")] // Admin xem báo cáo
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.Reports.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { success = true, data = reports });
        }

        // ===============================================
        // 4. ADMIN: DUYỆT / XỬ LÝ BÁO CÁO (REPORT)
        // ===============================================
        [Authorize(Roles = "Admin")]
        [HttpPut("reports/{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] UpdateReportStatusReq req)
        {
            // 1. Tìm báo cáo trong DB
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound(new { success = false, message = "Không tìm thấy báo cáo này" });

            // 2. Validate trạng thái (Chỉ nhận Resolved, Rejected, Pending)
            var validStatuses = new[] { "Resolved", "Rejected", "Pending" };
            if (!validStatuses.Contains(req.Status))
                return BadRequest(new { success = false, message = "Trạng thái không hợp lệ. Chỉ chấp nhận: Resolved, Rejected, Pending" });

            // Nếu trạng thái không thay đổi thì bỏ qua
            if (report.Status == req.Status)
                return Ok(new { success = true, message = $"Báo cáo đã ở trạng thái {req.Status} rồi." });

            // 3. Cập nhật trạng thái
            report.Status = req.Status;

            // 4. TẠO THÔNG BÁO CHO NGƯỜI GỬI REPORT
            if (report.ReportedByUserId != null) // Đảm bảo có ID người gửi
            {
                string notifTitle = "";
                string notifContent = "";

                if (req.Status == "Resolved")
                {
                    notifTitle = "✅ Báo cáo đã được xử lý";
                    notifContent = $"Cảm ơn bạn. Báo cáo (ID: #{id}) của bạn đã được hệ thống kiểm tra và xử lý thành công.";
                }
                else if (req.Status == "Rejected")
                {
                    notifTitle = "❌ Báo cáo bị từ chối";
                    notifContent = $"Báo cáo (ID: #{id}) của bạn đã bị hệ thống từ chối (do không vi phạm hoặc không đủ thông tin).";
                }

                // Chỉ gửi thông báo nếu trạng thái là Resolved hoặc Rejected
                if (!string.IsNullOrEmpty(notifTitle))
                {
                    var notif = new Notification
                    {
                        UserId = (Guid)report.ReportedByUserId,
                        Title = notifTitle,
                        Content = notifContent,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync(); // Lưu Notification trước để lấy ID

                    // 5. Bắn thông báo Real-time cho người dùng
                    Console.WriteLine($"[SIGNALR DEBUG] Đang bắn thông báo tới UserID: {report.ReportedByUserId}");
                    await _hubContext.Clients.User(report.ReportedByUserId.ToString()!).SendAsync("ReceiveNotification", new
                    {
                        id = notif.Id,
                        title = notif.Title,
                        content = notif.Content,
                        createdAt = notif.CreatedAt,
                        isRead = false
                    });
                }
                else
                {
                    // Nếu admin lùi về Pending thì chỉ cần save DB, không cần báo cáo
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Trường hợp báo cáo không có người gửi (lỗi data cũ), cứ save DB
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, message = $"Đã cập nhật trạng thái báo cáo thành: {req.Status}" });
        }

        // ==============================================================
        // CHO PHÉP USER TỰ NÂNG CẤP ROLE (DÙNG ĐỂ DEMO KHÔNG CẦN THANH TOÁN)
        // ==============================================================
        [Authorize] // Ai cũng gọi được
        [HttpPut("upgrade-role")]
        public async Task<IActionResult> UpgradeRole([FromBody] ChangeRoleRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (req == null || string.IsNullOrEmpty(req.Role))
                return BadRequest(new { success = false, message = "Role không được để trống" });

            // Gọi hàm đổi Role (Sếp nhớ đảm bảo IUserService có hàm này nhé)
            await _userService.ChangeUserRoleAsync(userId, req.Role);

            return Ok(new { success = true, message = $"Đã nâng cấp lên quyền {req.Role.ToUpper()} thành công!" });
        }
    }
}
