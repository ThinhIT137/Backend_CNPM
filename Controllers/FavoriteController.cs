using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]")]
    public class FavoriteController : Controller
    {
        private readonly IInteractionService _service;
        public FavoriteController(IInteractionService s) { _service = s; }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromBody] FavoriteRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdded = await _service.ToggleFavoriteAsync(userId, req);
            return Ok(new { success = true, isFavorite = isAdded });
        }

        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var data = await _service.GetMyFavoritesAsync(userId, page, pageSize);

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách yêu thích thành công",
                data = data
            });
        }
    }
}
