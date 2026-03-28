using backend.Models;

namespace backend.Services
{
    public interface IAuthService
    {
        public Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user);
        public Task ChangePasswordAsync(User user, string password, string Token, DateTime expired);

    }
}
