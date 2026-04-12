using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string BookingType { get; set; } = null!; // "hotel" hoặc "tour"
        public string ContactName { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public string? Note { get; set; }
        public string? ContactAddress { get; set; } // 🔴 THÊM CỘT NÀY VÀO NÈ
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded
        public string BookingStatus { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<Booking_Detail> BookingDetails { get; set; } = new List<Booking_Detail>();
    }
}
