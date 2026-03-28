using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITouristPlaceService
    {
        public Task<PagedResult<Tourist_Place>> GetPagedResult(int page = 1, int pageSize = 10);
        public Task<PagedResult<Tourist_Place>> GetPagedResult(int Tourist_Area_Id, int page = 1, int pageSize = 10);
    }
}
