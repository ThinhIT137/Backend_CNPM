using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITourService
    {
        public Task<PagedResult<Tour>> GetPagedResult(User? u, int page = 1, int pageSize = 10);
        public Task<PagedResult<Tour>> GetPagedResult(int Tourist_Area_Id, User? user, int page = 1, int pageSize = 10);
        public Task<PagedResult<Tour>> GetToursByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10);
        public Task<Tour> GetTourDetail(int tourId);
    }
}
