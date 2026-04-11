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
    }
}
