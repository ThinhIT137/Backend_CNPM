using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;
using backend.Exceptions;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly CnpmContext _context;

        public ProfileController(CnpmContext context)
        {
            _context = context;
        }

        // Lấy thông tin cá nhân của người đang đăng nhập
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy ID từ JWT Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedException("Người dùng không hợp lệ.");
            }

            var user = await _context.Users
                .AsNoTracking() // Tối ưu hiệu suất truy vấn vì chỉ để đọc dữ liệu
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    avt = user.Avt,
                    role = user.Role,
                    createdAt = user.CreatedAt
                }
            });
        }

        // Cập nhật thông tin cá nhân (Tên, Ảnh đại diện)
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            // Chỉ cập nhật những trường có gửi lên
            if (!string.IsNullOrEmpty(req.Name))
            {
                user.Name = req.Name;
            }
            if (!string.IsNullOrEmpty(req.Avt))
            {
                user.Avt = req.Avt;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Cập nhật hồ sơ thành công.",
                data = new
                {
                    name = user.Name,
                    avt = user.Avt
                }
            });
        }

        // Đổi mật khẩu (dành cho người dùng đang đăng nhập và nhớ mật khẩu cũ)
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            // Kiểm tra mật khẩu cũ có khớp không
            bool isValidOldPassword = BCryptNet.Verify(req.OldPassword, user.PasswordHash);
            if (!isValidOldPassword)
            {
                return BadRequest(new { success = false, message = "Mật khẩu cũ không chính xác." });
            }

            // Kiểm tra mật khẩu mới có trùng mật khẩu cũ không
            if (req.OldPassword == req.NewPassword)
            {
                return BadRequest(new { success = false, message = "Mật khẩu mới không được trùng với mật khẩu cũ." });
            }

            // Mã hóa và lưu mật khẩu mới
            user.PasswordHash = BCryptNet.HashPassword(req.NewPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Tùy chọn: Bạn có thể thu hồi tất cả Refresh Token cũ ở đây để ép đăng nhập lại trên các thiết bị khác

            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        [Authorize] // Yêu cầu đăng nhập vì đang ở trang Profile
        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Subject) || string.IsNullOrWhiteSpace(req.Message))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ Chủ đề và Nội dung." });
            }

            // Lấy ID của user đang đăng nhập
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Người dùng không hợp lệ." });
            }

            var feedback = new Feedback
            {
                UserId = userId,
                Subject = req.Subject,
                Message = req.Message,
                Status = "New", // Mặc định là thư mới
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cảm ơn bạn đã gửi ý kiến đóng góp!" });
        }
    }
}
