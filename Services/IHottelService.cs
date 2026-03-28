using backend.Models;

namespace backend.Services
{
    public interface IHottelService
    {
        Task<List<Tourist_Area>> GetTrendingTouristAreasAsync(User u, int page = 1, int pageSize = 10);
        Task<List<Tourist_Area>> GetTrendingTouristAreasAsync(int page = 1, int pageSize = 10);
    }
}
