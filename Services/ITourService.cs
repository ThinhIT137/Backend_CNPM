using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface ITourService
    {
        public Task<PagedResult<Tour>> GetPagedResult(int page = 1, int pageSize = 10);
        public Task<PagedResult<Tour>> GetPagedResult(int Tourist_Area_Id, int page = 1, int pageSize = 10);
    }
}
