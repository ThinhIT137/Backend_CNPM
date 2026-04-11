using backend.DTO;
using backend.Models;
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

        public AdminController(CnpmContext context)
        {
            _context = context;
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
        [Authorize] // User gửi báo cáo
        [HttpPost("reports")]
        public async Task<IActionResult> SubmitReport([FromBody] ReportRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var report = new Report
            {
                ReportedByUserId = userId,
                EntityType = req.EntityType,
                EntityId = req.EntityId,
                Reason = req.Reason,
                Description = req.Description,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã gửi báo cáo cho Admin" });
        }

        [Authorize(Roles = "Admin")] // Admin xem báo cáo
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.Reports.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { success = true, data = reports });
        }
    }
}
