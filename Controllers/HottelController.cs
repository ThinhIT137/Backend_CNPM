using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class HottelController : Controller
    {
        private readonly IHottelService _hottelService;
        private readonly CnpmContext _context;

        public HottelController(IHottelService hottelService, CnpmContext context)
        {
            _hottelService = hottelService;
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail([FromQuery] int id)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
