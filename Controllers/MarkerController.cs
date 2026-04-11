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
    public class MarkerController : Controller
    {
        private readonly IInteractionService _service;
        private readonly CnpmContext _context;
        public MarkerController(IInteractionService s, CnpmContext c) { _service = s; _context = c; }

        [Authorize]
        [HttpPost]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMarker([FromBody] MarkerRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int newId = await _service.CreateMarkerAsync(userId, req);

            return Ok(new
            {
                success = true,
                message = "Đã lưu vị trí thành công",
                data = new { id = newId } // Đút ID vô đây
            });
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMarker(int id, [FromBody] MarkerRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.UpdateMarkerAsync(userId, id, req);

            return Ok(new
            {
                success = true,
                message = "Đã cập nhật vị trí thành công"
            });
        }

        // 🔴 THÊM MỚI: API XÓA MARKER
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMarker(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.DeleteMarkerAsync(userId, id);

            return Ok(new
            {
                success = true,
                message = "Đã xóa vị trí thành công"
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyMarkers([FromQuery] string? keyword = null)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var data = await _service.GetMyMarkersAsync(userId, keyword);
            return Ok(new
            {
                success = true,
                data
            });
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicMarkers()
        {
            // Lấy tất cả marker được public của mọi người để hiện lên bản đồ chung
            var markers = await _context.Markers.Where(m => m.IsPublic == true).ToListAsync();
            return Ok(new { success = true, data = markers });
        }
    }
}
