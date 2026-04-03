using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface IHottelService
    {
        public Task<object> GetTrendingHottelAsync(int page = 1, int pageSize = 10);
        public Task<object> GetTrendingHottelAsync(User u, int page = 1, int pageSize = 10);
        public Task<PagedResult<Hottel>> GetHotelsByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10);
        public Task<object> GetHotelDetailAsync(int id, User? user);
    }
}
