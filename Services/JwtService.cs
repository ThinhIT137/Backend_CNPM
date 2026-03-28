using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace backend.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // Tạo Access Token
        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var secret = _config["Jwt:Key"] ?? throw new Exception("Jwt:Key is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "15");

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Tạo Refresh Token
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        // Hash Refresh Token
        public string HashRefreshToken(string refreshToken)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        // Thêm RefreshToken vào database
        public async Task SaveRefreshTokenAsync(CnpmContext _contex, User user, string refreshToken)
        {
            var refreshTokenHash = HashRefreshToken(refreshToken);
            var days = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");

            var refreshEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.Now
            };

            _contex.RefreshTokens.Add(refreshEntity);
            await _contex.SaveChangesAsync();
        }

        public async Task<bool> ValidateRefreshTokenAsync(CnpmContext _context, Guid userId, string refreshToken)
        {
            var refresh = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == HashRefreshToken(refreshToken));
            if (refresh == null)
            {
                return false;
            }
            if (refresh.UserId != userId)
            {
                return false;
            }
            if (refresh.IsRevoked)
            {
                return false;
            }
            if (refresh.ExpiresAt < DateTime.Now)
            {
                refresh.IsRevoked = true;
                _context.RefreshTokens.Update(refresh);
                await _context.SaveChangesAsync();
                return false;
            }

            refresh.IsRevoked = true;
            _context.RefreshTokens.Update(refresh);
            await _context.SaveChangesAsync();
            return true;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,

                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
            };

            var tokenHandle = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandle.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedException("Token không hợp lệ hoặc sai thuật toán mã hóa!");
            }
            return principal;
        }

        public Guid? GetUserIdFromToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new NotFoundException("Không tìm thấy Access Token");

            var principal = GetPrincipalFromExpiredToken(accessToken);

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                throw new UnauthorizedException("Token rác!");

            return Guid.Parse(userIdStr);
        }
    }


}
