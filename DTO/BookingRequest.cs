using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class BookingRequest
    {
        [Required] public string BookingType { get; set; } = null!; // Bắt buộc truyền "Hotel" hoặc "Tour"

        [Required] public string ContactName { get; set; } = null!;
        [Required] public string ContactPhone { get; set; } = null!;
        public string? Note { get; set; }

        // --- DÀNH CHO HOTEL ---
        public List<int>? HotelRoomIds { get; set; } // Khách chọn phòng nào thì ném ID phòng đó vào đây

        // --- DÀNH CHO TOUR ---
        public int? TourDepartureId { get; set; } // ID của chuyến đi
        public List<string>? SeatNumbers { get; set; } // Khách ghép chọn ghế (VD: ["1", "2"])
        public bool IsPrivateTour { get; set; } = false; // Check = true nếu khách đòi bao xe
    }
}
