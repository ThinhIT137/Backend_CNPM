using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly CnpmContext _context;

        public ImageController(CnpmContext context)
        {
            _context = context;
        }

        [HttpPost("save-link")]
        public async Task<IActionResult> SaveImageLink([FromBody] ImageLinkRequest req)
        {
            if (string.IsNullOrEmpty(req.Url)) return BadRequest("Thiếu Link ảnh rồi sếp ơi!");

            // Nếu là ảnh bìa, reset các ảnh bìa cũ
            if (req.IsCover)
            {
                var oldCovers = _context.Imgs.Where(i => i.EntityType == req.EntityType && i.EntityId == req.EntityId && i.IsCover);
                foreach (var oc in oldCovers) oc.IsCover = false;
            }

            var imgRecord = new Img
            {
                url = req.Url,
                IsCover = req.IsCover,
                EntityType = req.EntityType,
                EntityId = req.EntityId,
                CreatedAt = DateTime.Now
            };

            _context.Imgs.Add(imgRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Lưu link ảnh thành công!", data = imgRecord });
        }

        // 1. API XÓA ẢNH
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _context.Imgs.FindAsync(id);
            if (img == null) return NotFound("Không tìm thấy ảnh này!");

            // Xóa trong Database (Chưa xóa trên Supabase để an toàn, nếu muốn xóa sếp nghiên cứu thêm hàm remove của supabase sau nha)
            _context.Imgs.Remove(img);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa ảnh khỏi DB" });
        }

        // 2. API SET LẠI ẢNH BÌA CHO ẢNH CŨ
        [HttpPut("set-cover/{id}")]
        public async Task<IActionResult> SetCoverImage(int id, [FromQuery] string entityType, [FromQuery] int entityId)
        {
            // Tắt hết cover của các ảnh cũ cùng chủ thớt
            var oldCovers = _context.Imgs.Where(i => i.EntityType == entityType && i.EntityId == entityId && i.IsCover);
            foreach (var oc in oldCovers) oc.IsCover = false;

            // Bật cover cho thằng được chọn
            var newCover = await _context.Imgs.FindAsync(id);
            if (newCover != null) newCover.IsCover = true;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã set ảnh bìa thành công" });
        }
    }
}