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
            // Lấy tất cả marker được public
            var publicMarkers = await _context.Markers
                .Where(m => m.IsPublic == true)
                .AsNoTracking()
                .ToListAsync();

            var markerIds = publicMarkers.Select(m => m.Id).ToList();

            // Tìm ảnh của các marker đó
            var images = await _context.Imgs
                .Where(img => img.EntityType == "marker" && markerIds.Contains(img.EntityId))
                .AsNoTracking()
                .ToListAsync();

            // Đóng gói data trả về (cấu trúc y chang GetMyMarkers cho nó đồng nhất)
            var dataResult = publicMarkers.Select(m => new
            {
                id = m.Id,
                title = m.Title,
                description = m.Description,
                latitude = m.Latitude,
                longitude = m.Longitude,
                isPublic = m.IsPublic,
                tourist_Place_Id = m.TouristPlaceId,
                images = images.Where(img => img.EntityId == m.Id).Select(img => new { id = img.Id, url = img.url, isCover = img.IsCover }).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == m.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return Ok(new { success = true, data = dataResult });
        }

        // ===============================================
        // ADMIN: LẤY TẤT CẢ MARKER TRONG HỆ THỐNG
        // ===============================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllMarkersForAdmin()
        {
            var markers = await _context.Markers
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var markerIds = markers.Select(m => m.Id).ToList();

            var images = await _context.Imgs
                .Where(img => img.EntityType == "marker" && markerIds.Contains(img.EntityId))
                .AsNoTracking()
                .ToListAsync();

            var dataResult = markers.Select(m => new
            {
                id = m.Id,
                title = m.Title,
                description = m.Description,
                latitude = m.Latitude,
                longitude = m.Longitude,
                isPublic = m.IsPublic,
                tourist_Place_Id = m.TouristPlaceId,
                images = images.Where(img => img.EntityId == m.Id).Select(img => new { id = img.Id, url = img.url, isCover = img.IsCover }).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == m.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return Ok(new { success = true, data = dataResult });
        }

        // Dùng đường dẫn khác để tránh đụng độ với cái Delete của User
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id:int}")]
        public async Task<IActionResult> AdminDeleteMarker(int id)
        {
            var marker = await _context.Markers.FindAsync(id);
            if (marker == null) return NotFound(new { success = false, message = "Không tìm thấy Marker" });

            _context.Markers.Remove(marker);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Admin đã xóa Marker thành công" });
        }
    }
}
