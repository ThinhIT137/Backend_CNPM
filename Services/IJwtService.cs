using backend.Models;
using System.Security.Claims;

namespace backend.Services
{
    public interface IJwtService
    {
        public string GenerateAccessToken(User user);
        public string GenerateRefreshToken();
        public string HashRefreshToken(string refreshToken);
        public Task SaveRefreshTokenAsync(CnpmContext _contex, User user, string refreshToken);
        public Task<bool> ValidateRefreshTokenAsync(CnpmContext _context, Guid userId, string refreshToken);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public Guid? GetUserIdFromToken(string accessToken);
    }
}
