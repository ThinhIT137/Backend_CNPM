using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class BookingRequest
    {
        [Required] public string BookingType { get; set; } = null!;
        [Required] public string ContactName { get; set; } = null!;
        [Required] public string ContactPhone { get; set; } = null!;
        [Required] public string? ContactAddress { get; set; }
        public string? Note { get; set; }
        public List<int>? HotelRoomIds { get; set; }
        public int? TourDepartureId { get; set; }
        public List<string>? SeatNumbers { get; set; }
        public bool IsPrivateTour { get; set; } = false;
    }
}
