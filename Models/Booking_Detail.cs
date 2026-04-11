using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Booking_Detail
    {
        [Key]
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int? HotelRoomId { get; set; }
        public int? TourDepartureId { get; set; }
        public string? SeatNumber { get; set; } // VD: "1A"
        public bool IsPrivateTour { get; set; } = false; // Đánh dấu nếu là Tour "Bao nguyên chuyến"

        public decimal UnitPrice { get; set; } // Giá tại thời điểm đặt

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }
        [ForeignKey("HotelRoomId")]
        public virtual Hotel_Room? HotelRoom { get; set; }
        [ForeignKey("TourDepartureId")]
        public virtual Tour_Departure? TourDeparture { get; set; }
    }
}
