using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AdminController(CnpmContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
                End_date = DateTime.Now.AddDays(30),
                IsActive = true
            };
            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Tạo quảng cáo thành công" });
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
