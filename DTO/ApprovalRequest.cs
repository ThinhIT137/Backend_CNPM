using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class ApprovalRequest
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string Status { get; set; } = null!; // Sẽ nhận: "Approved" hoặc "Rejected"
    }
}
