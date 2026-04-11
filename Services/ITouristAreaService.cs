using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITouristAreaService
    {
        public Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(User u, int page = 1, int pageSize = 10);
        public Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(int page = 1, int pageSize = 10);
        public Task<TouristAreaDetailResponse> GetDetailTouristAreasAsync(int id, string type, int page = 1, int pageSize = 10);
        public Task<TouristAreaDetailResponse> GetDetailTouristAreasAsync(int id, string type, User user, int page = 1, int pageSize = 10);
        public Task update_click_tourist_area(int Tourist_Area_Id, User user);
        public Task UpdateTouristArea(int id, TouristAreaRequest tourist);
        public Task<int> addTouristArea(Tourist_Area tourist);
        public Task RemoveTouristArea(int id);
        public Task<object> GetMyTouristAreasAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null);
        public Task<object> GetAllForDropdownAsync();

    }
}
