using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using BCryptNet = BCrypt.Net.BCrypt;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly CnpmContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthService(CnpmContext context, IJwtService jwtService, IConfiguration config)
        {
            _config = config;
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.SaveRefreshTokenAsync(_context, user, refreshToken);

            return (accessToken, refreshToken);
        }
        public async Task ChangePasswordAsync(User user, string password, string Token, DateTime expired)
        {
            if (user == null)
            {
                throw new NotFoundException("No account");
            }

            if (user.ResetPasswordToken != Token)
            {
                throw new BadRequestException("Mã xác nhận không đúng hoặc đã bị sửa!");
            }

            if (user.ResetPasswordExpiry < expired)
            {
                throw new BadRequestException("Mã xác nhận đã hết hạn!");
            }

            var rfUser = await _context.RefreshTokens.Where(rf => rf.UserId == user.Id && rf.IsRevoked == false).ToListAsync();
            foreach (var rf in rfUser)
            {
                rf.IsRevoked = true;
            }

            user.ResetPasswordExpiry = null;
            user.ResetPasswordToken = null;
            user.PasswordHash = BCryptNet.HashPassword(password);
            await _context.SaveChangesAsync();
        }
    }
}

