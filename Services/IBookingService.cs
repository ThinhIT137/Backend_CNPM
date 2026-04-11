using backend.DTO;

namespace backend.Services
{
    public interface IBookingService
    {
        public Task CreateBookingAsync(Guid userId, BookingRequest req);
    }
}
