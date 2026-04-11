using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; } // Có thể null nếu cho khách vãng lai góp ý
        public string Subject { get; set; } // Chủ đề: Báo lỗi app, Góp ý tính năng...
        public string Message { get; set; } // Nội dung chi tiết
        public string Status { get; set; } // Trạng thái: New, Reviewed (Đã xem)
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
