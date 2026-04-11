using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tour_Departure
    {
        [Key]
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime StartDate { get; set; } // Ngày khởi hành
        public int TotalSeats { get; set; }     // Tổng số ghế (VD: 45 ghế)
        public int AvailableSeats { get; set; } // Số ghế còn trống

        // Lưu sơ đồ ghế đã đặt dạng mảng JSON (VD: ["1A", "1B", "2C"]) để query cho lẹ
        // Hoặc ông có thể tách thành 1 bảng Tour_Seat riêng nếu muốn quản lý phức tạp hơn
        public string? BookedSeats { get; set; }

        public string Status { get; set; } = "Open"; // Open, Full, Cancelled

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }
    }
}
