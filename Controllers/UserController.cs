using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")] // Chốt chặn an ninh vòng ngoài
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

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

        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            await _userService.ToggleUserStatusAsync(id);

            return Ok(new { success = true, message = "Cập nhật trạng thái tài khoản thành công" });
        }

        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Role))
                return BadRequest(new { success = false, message = "Role không được để trống" });

            await _userService.ChangeUserRoleAsync(id, req.Role);

            return Ok(new { success = true, message = $"Đã cấp quyền {req.Role} thành công" });
        }
    }
}
