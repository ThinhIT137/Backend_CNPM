using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITouristPlaceService
    {
        public Task<PagedResult<Tourist_Place>> GetPagedResult(int page = 1, int pageSize = 10);
        public Task<PagedResult<Tourist_Place>> GetPagedResult(int Tourist_Area_Id, int page = 1, int pageSize = 10);
        public Task<PagedResult<Tourist_Place>> GetPagedResult(User u, int page = 1, int pageSize = 10);
        public Task<object> GetMyTouristPlacesAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null);
        public Task<int> CreateTouristPlaceAsync(TouristPlaceRequest req, Guid ownerId);
        public Task UpdateTouristPlaceAsync(int id, TouristPlaceRequest req, Guid ownerId);
        public Task DeleteTouristPlaceAsync(int id, Guid ownerId);
        public Task<object> GetAllForDropdownAsync();
    }
}
