using backend.DTO;

namespace backend.Services
{
    public interface ISearchService
    {
        public Task<object> FilterAsync(SearchFilterRequest req);
    }
}
