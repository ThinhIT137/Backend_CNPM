
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService : IUserService
    {
        public readonly CnpmContext _context;

        public UserService(CnpmContext context)
        {
            _context = context;
        }

        public async Task ChangeUserRoleAsync(Guid id, string newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng này");

            var validRoles = new[] { "Admin", "Owner", "User" };
            if (!validRoles.Contains(newRole))
                throw new BadRequestException("Quyền (Role) không hợp lệ. Chỉ chấp nhận: Admin, Owner, User");

            user.Role = newRole;
            await _context.SaveChangesAsync();
        }

        // Lấy danh sách (Phân trang)
        public async Task<PagedResult<UserAdminResponse>> GetPagedUsersAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserAdminResponse // Đổi ở đây
                {
                    Id = u.Id,
                    Name = u.Name,
                    Avt = u.Avt, // Thêm dòng này nếu Admin muốn xem avatar
                    Email = u.Email,
                    Role = u.Role,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt
                })
                .AsNoTracking().AsSplitQuery()
                .ToListAsync();

            return new PagedResult<UserAdminResponse> // Và đổi ở đây
            {
                Items = users,
                TotalCount = totalCount,
                TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task ToggleUserStatusAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng này");

            // Đang bị khóa -> Mở khóa
            if (string.IsNullOrEmpty(user.Status) || !user.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                user.Status = "Active";
            }
            else // Đang Active -> Khóa mõm
            {
                user.Status = "Banned";

                // Tuyệt kỹ: Phế võ công - Thu hồi toàn bộ Refresh Token để nó bị văng ra ngay lập tức
                var refreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == user.Id && !rt.IsRevoked).ToListAsync();
                foreach (var token in refreshTokens)
                {
                    token.IsRevoked = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
