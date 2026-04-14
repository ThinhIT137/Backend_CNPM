using backend.DTO;
using backend.Models;

namespace backend.Services
{
    public interface IInteractionService
    {
        public Task AddReviewAsync(Guid userId, ReviewRequest req);
        public Task<bool> ToggleFavoriteAsync(Guid userId, FavoriteRequest req);
        public Task<int> CreateMarkerAsync(Guid userId, MarkerRequest req);
        public Task<object> GetMyMarkersAsync(Guid userId, string? keyword = null);
        public Task<object> GetReviewsReceivedAsync(Guid ownerId, int page = 1, int pageSize = 10);
        public Task UpdateMarkerAsync(Guid userId, int markerId, MarkerRequest req);
        public Task DeleteMarkerAsync(Guid userId, int markerId);
        public Task<object> GetMyFavoritesAsync(Guid userId, int page = 1, int pageSize = 10);
    }
}
