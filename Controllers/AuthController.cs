using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using BCryptNet = BCrypt.Net.BCrypt;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CnpmContext _context;
        private readonly IJwtService _jwtService;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        private readonly int AccessTokenMinutes;
        private readonly int RefreshTokenDays;

        public AuthController(CnpmContext context, IConfiguration config, IJwtService jwtService, IAuthService authService, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _jwtService = jwtService;
            _authService = authService;
            _emailService = emailService;

            AccessTokenMinutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "15");
            RefreshTokenDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null) // kiểm tra tài khoản email tồn tại không
            {
                throw new BadRequestException("No account");
            }

            if (string.IsNullOrEmpty(user.Status) || !user.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!");
            }

            bool isValidPassword = BCryptNet.Verify(req.Password, user.PasswordHash);

            if (!isValidPassword) // Kiểm tra password
            {
                throw new UnauthorizedException("Wrong password");
            }

            var (accessToken, refreshToken) = await _authService.IssueTokensAsync(user);
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(RefreshTokenDays)
            });

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.Now.AddMinutes(AccessTokenMinutes),
                info = new UserResponse
                {
                    name = user.Name,
                    email = user.Email,
                    avt = user.Avt,
                    role = user.Role
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var u = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (u != null)
            {
                throw new BadRequestException("Tài khoản đã tồn tại");
            }

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = req.Email,
                PasswordHash = BCryptNet.HashPassword(req.PasswordHash),
                Name = req.Name,
                Avt = "/Img/User_Icon.png",
                Role = "User",
                CreatedAt = DateTime.Now,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (accessToken, refreshToken) = await _authService.IssueTokensAsync(user);
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(RefreshTokenDays)
            });

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.Now.AddMinutes(AccessTokenMinutes),
                info = new UserResponse
                {
                    name = user.Name,
                    email = user.Email,
                    avt = user.Avt,
                    role = user.Role
                }
            });
        }

        [Authorize]
        [HttpPost("LogOut")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);

                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
            }

            Response.Cookies.Append("RefreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(-1)
            });
            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            // kiểm tra access token
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedException("Không tìm thấy Access Token cũ");
            }
            var oldAccessToken = authHeader.Replace("Bearer ", "");
            var principal = _jwtService.GetPrincipalFromExpiredToken(oldAccessToken);

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new UnauthorizedException("Token rác!");
            }

            var userId = Guid.Parse(userIdStr);
            var isValid = await _jwtService.ValidateRefreshTokenAsync(_context, userId, refreshToken);
            // kiểm tra refresh token
            if (!isValid) throw new UnauthorizedException("Refresh Token đã hết hạn hoặc không hợp lệ!");
            Console.WriteLine("3");
            var user = await _context.Users.FindAsync(userId);
            var (newAccessToken, newRefreshToken) = await _authService.IssueTokensAsync(user);

            // cấp mới
            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(RefreshTokenDays)
            });

            Console.WriteLine("4");

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                ExpiresAt = DateTime.Now.AddMinutes(AccessTokenMinutes),
                info = new UserResponse
                {
                    name = user.Name,
                    email = user.Email,
                    avt = user.Avt,
                    role = user.Role
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Ok(new { success = true, message = "Email hợp lệ đã gửi thư!" });

            string resetToken = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(25);
            await _context.SaveChangesAsync();
            var expiry = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeMilliseconds();
            var resetLink = $"http://localhost:3000/resetPassword?email={user.Email}&token={resetToken}&expiry={expiry}";
            var emailSubject = "Khôi phục mật khẩu ứng dụng app du lịch";
            var emailBody = $"<h1>Chào {user.Name}, </h1><p>Bấm vào link sau để đổi mật khẩu (có hạn 10 phút): <a href='{resetLink}'>Đổi mật khẩu</a></p>";

            await _emailService.SendEmailAsync(user, emailSubject, emailBody);

            return Ok(new { success = true, message = "Đã gửi email khôi phục thành công" });
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            await _authService.ChangePasswordAsync(user, req.Password, req.Token, DateTime.Now);
            var (accessToken, refreshToken) = await _authService.IssueTokensAsync(user);
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(RefreshTokenDays)
            });

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.Now.AddMinutes(AccessTokenMinutes),
                info = new UserResponse
                {
                    name = user.Name,
                    email = user.Email,
                    avt = user.Avt,
                    role = user.Role
                }
            });
        }
    }
}
