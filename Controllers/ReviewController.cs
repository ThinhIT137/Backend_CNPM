using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]")]
    public class ReviewController : Controller
    {
        private readonly IInteractionService _service;
        private readonly CnpmContext _context;
        public ReviewController(IInteractionService s, CnpmContext c) { _service = s; _context = c; }

        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.AddReviewAsync(userId, req);
            return Ok(new { success = true, message = "Đánh giá thành công" });
        }

        [AllowAnonymous]
        [HttpGet("{type}/{id}")] // GET /api/Review/hotel/5
        public async Task<IActionResult> GetReviews(string type, int id)
        {
            var list = await _context.Reviews
                .Include(r => r.user)
                .Where(r => r.EntityType == type.ToLower() && r.EntityId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserName = r.user.Name,
                    UserAvt = r.user.Avt,
                    Star = r.Score,
                    Content = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
            return Ok(new { success = true, data = list });
        }

        [Authorize(Roles = "Owner, Admin")]
        [HttpGet("owner-stats")]
        public async Task<IActionResult> GetOwnerReviewStats()
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Tìm tất cả Hotel và Tour của Owner này
            var myHotelIds = await _context.Hotels.Where(h => h.Created_By_UserId == ownerId).Select(h => h.Id).ToListAsync();
            var myTourIds = await _context.Tours.Where(t => t.Created_By_UserId == ownerId).Select(t => t.Id).ToListAsync();

            // Lấy tất cả Review thuộc về Khách sạn/Tour của Owner này
            var myReviews = await _context.Reviews
                .Where(r => (r.EntityType == "hotel" && myHotelIds.Contains(r.EntityId.Value)) ||
                            (r.EntityType == "tour" && myTourIds.Contains(r.EntityId.Value)))
                .ToListAsync();

            var totalReviews = myReviews.Count;
            var averageScore = totalReviews > 0 ? Math.Round(myReviews.Average(r => r.Score), 1) : 0;

            // Đếm số lượng từng loại sao (1 sao, 2 sao... 5 sao)
            var starCounts = new
            {
                Star5 = myReviews.Count(r => r.Score == 5),
                Star4 = myReviews.Count(r => r.Score == 4),
                Star3 = myReviews.Count(r => r.Score == 3),
                Star2 = myReviews.Count(r => r.Score == 2),
                Star1 = myReviews.Count(r => r.Score == 1)
            };

            return Ok(new { success = true, data = new { totalReviews, averageScore, starCounts } });
        }
    }
}
