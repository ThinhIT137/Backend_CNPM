using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class HotelController : Controller
    {
        private readonly IHotelService _hottelService;
        private readonly CnpmContext _context;

        public HotelController(IHotelService hottelService, CnpmContext context)
        {
            _hottelService = hottelService;
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            Guid? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid parsedId)) userId = parsedId;
            }

            object data;

            if (userId.HasValue)
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (currentUser != null)
                {
                    data = await _hottelService.GetTrendingHottelAsync(currentUser, page, pageSize);
                }
                else
                {
                    data = await _hottelService.GetTrendingHottelAsync(page, pageSize);
                }
            }
            else
            {
                data = await _hottelService.GetTrendingHottelAsync(page, pageSize);
            }

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách khách sạn thành công",
                data = data
            });
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] int id)
        {
            User? user = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid parsedId)) user = await _context.Users.Where(u => u.Id == parsedId).FirstOrDefaultAsync();
            }

            var data = await _hottelService.GetHotelDetailAsync(id, user);

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin khách sạn thành công",
                data = data
            });
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                throw new UnauthorizedException("Vui lòng đăng nhập");
            return userId;
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] HotelRequest req)
        {
            int newHotelId = await _hottelService.CreateHotelAsync(req, GetCurrentUserId());
            return Ok(new { success = true, message = "Thêm khách sạn thành công (Chờ duyệt)", data = new { id = newHotelId } });
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelRequest req)
        {
            await _hottelService.UpdateHotelAsync(id, req, GetCurrentUserId());
            return Ok(new { success = true, message = "Cập nhật khách sạn thành công" });
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            await _hottelService.DeleteHotelAsync(id, GetCurrentUserId());
            return Ok(new { success = true, message = "Xóa khách sạn thành công" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id:int}")]
        public async Task<IActionResult> ApproveHotel(int id, [FromBody] ApprovalRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Status))
                throw new BadRequestException("Dữ liệu không hợp lệ");

            await _hottelService.ApproveHotelAsync(id, req.Status);

            return Ok(new
            {
                success = true,
                message = $"Đã chuyển trạng thái Khách sạn thành: {req.Status}"
            });
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpPost("{hotelId}/rooms")]
        public async Task<IActionResult> AddRoom(int hotelId, [FromBody] RoomRequest req)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null) return NotFound("Khách sạn không tồn tại");
            if (hotel.Created_By_UserId != GetCurrentUserId()) return StatusCode(403, "Đây không phải khách sạn của bạn");

            var room = new Hotel_Room
            {
                HotelId = hotelId,
                RoomName = req.RoomName,
                Floor = req.Floor,
                RoomType = req.RoomType,
                Price = req.Price,
                Status = "Available"
            };
            _context.HotelRooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Thêm phòng thành công" });
        }

        // ==========================================
        // LẤY DANH SÁCH KHÁCH SẠN CỦA TÔI
        // GET: /api/Hotel/my-hotels
        // ==========================================
        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpGet("my-hotels")]
        public async Task<IActionResult> GetMyHotels([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            var userId = GetCurrentUserId();
            var data = await _hottelService.GetMyHotelsAsync(userId, page, pageSize, keyword, status);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách khách sạn thành công",
                data = data
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetAllPendingHotels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Hotels.Where(h => h.Status == "Pending");

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(h => h.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Lấy ảnh bìa cho đống này (nếu cần show ra UI)
                var hotelIds = items.Select(h => h.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId) && img.IsCover)
                    .ToListAsync();

                var dataResult = items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    address = a.Address,
                    rating_average = a.RatingAverage,
                    status = a.Status,
                    createdAt = a.CreatedAt,
                    coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id)?.url ?? "/Img/ImgNull.jpg"
                });

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách khách sạn chờ duyệt thành công",
                    data = new
                    {
                        items = dataResult,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        currentPage = page
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpPut("rooms/{roomId:int}")]
        public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] RoomRequest req)
        {
            var room = await _context.HotelRooms.FindAsync(roomId);
            if (room == null) return NotFound("Không tìm thấy phòng");

            // Cập nhật thông tin
            room.RoomName = req.RoomName;
            room.Floor = req.Floor;
            room.RoomType = req.RoomType;
            room.Price = req.Price;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật phòng thành công" });
        }

        [Authorize(Roles = "Owner, Admin, Hotel")]
        [HttpDelete("rooms/{roomId:int}")]
        public async Task<IActionResult> DeleteHotelRoom(int roomId)
        {
            var room = await _context.HotelRooms.FindAsync(roomId);
            if (room == null) return NotFound("Không tìm thấy phòng");

            _context.HotelRooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Xóa phòng thành công" });
        }
    }
}
