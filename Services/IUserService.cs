using backend.DTO;

namespace backend.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserAdminResponse>> GetPagedUsersAsync(int page = 1, int pageSize = 10);
        Task ToggleUserStatusAsync(Guid id);
        Task ChangeUserRoleAsync(Guid id, string newRole);
    }
}
