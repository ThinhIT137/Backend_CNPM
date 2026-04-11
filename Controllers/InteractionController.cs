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
    public class InteractionController : Controller
    {
        private readonly IInteractionService _service;
        private readonly CnpmContext _context;
        public InteractionController(IInteractionService s, CnpmContext c) { _service = s; _context = c; }

        // ==========================================
        // LẤY DANH SÁCH CÁC ĐÁNH GIÁ CỦA TÔI (GỌI SERVICE)
        // GET: /api/Interaction/my-reviews
        // ==========================================
        [Authorize]
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var ownerId = Guid.Parse(userIdStr);

            // Giao hết việc nặng nhọc cho Service
            var data = await _service.GetReviewsReceivedAsync(ownerId, page, pageSize);

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách đánh giá nhận được thành công",
                data = data
            });
        }
    }
}
