using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface IHotelService
    {
        public Task<object> GetTrendingHottelAsync(int page = 1, int pageSize = 10);
        public Task<object> GetTrendingHottelAsync(User u, int page = 1, int pageSize = 10);
        public Task<PagedResult<Hotel>> GetHotelsByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10);
        public Task<object> GetHotelDetailAsync(int id, User? user);
        public Task<int> CreateHotelAsync(HotelRequest req, Guid ownerId);
        public Task UpdateHotelAsync(int id, HotelRequest req, Guid ownerId);
        public Task DeleteHotelAsync(int id, Guid ownerId);
        public Task ApproveHotelAsync(int id, string status);
        public Task<object> GetMyHotelsAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null);
    }
}
