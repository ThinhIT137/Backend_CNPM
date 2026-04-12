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
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly CnpmContext _context;

        public UserController(IUserService userService, CnpmContext context)
        {
            _userService = userService;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("list")]
        public async Task<IActionResult> GetListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Cứ yên tâm viết, không cần try-catch vì đã có ExceptionMiddleware
            var result = await _userService.GetPagedUsersAsync(page, pageSize);

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách người dùng thành công",
                data = result
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            await _userService.ToggleUserStatusAsync(id);

            return Ok(new { success = true, message = "Cập nhật trạng thái tài khoản thành công" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Role))
                return BadRequest(new { success = false, message = "Role không được để trống" });

            await _userService.ChangeUserRoleAsync(id, req.Role);

            return Ok(new { success = true, message = $"Đã cấp quyền {req.Role} thành công" });
        }

        [Authorize]
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

        [Authorize] // Ai đăng nhập cũng gọi được
        [HttpPut("upgrade-role")]
        public async Task<IActionResult> UpgradeRole([FromBody] ChangeRoleRequest req)
        {
            // Lấy ID của người đang đăng nhập
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            if (req == null || string.IsNullOrEmpty(req.Role))
                return BadRequest(new { success = false, message = "Role không được để trống" });

            // GỌI HÀM UPDATE CỦA SERVICE SẴN CÓ
            await _userService.ChangeUserRoleAsync(userId, req.Role);

            return Ok(new { success = true, message = $"Đã nâng cấp lên quyền {req.Role.ToUpper()} thành công!" });
        }
    }
}
