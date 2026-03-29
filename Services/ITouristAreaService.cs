using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITouristAreaService
    {
        public Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(User u, int page = 1, int pageSize = 10);
        public Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(int page = 1, int pageSize = 10);
        public Task<TouristAreaDetailResponse> GetDetailTouristAreasAsync(int id, string type, int page = 1, int pageSize = 10);
    }
}
