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
        public Task<int> CreateTourAsync(TourRequest req, Guid ownerId);
        public Task UpdateTourAsync(int id, TourRequest req, Guid ownerId);
        public Task DeleteTourAsync(int id, Guid ownerId);
        public Task AddItineraryAsync(int tourId, TourItineraryRequest req, Guid ownerId);
        public Task UpdateItineraryAsync(int itineraryId, TourItineraryRequest req, Guid ownerId);
        public Task DeleteItineraryAsync(int itineraryId, Guid ownerId);
        public Task ApproveTourAsync(int id, string status);
        public Task<object> GetMyToursAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null);
    }
}
