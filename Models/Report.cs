using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // Pending, Approved, Rejected
        public Guid? ReportedByUserId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ReportedByUserId")]
        public virtual User? User { get; set; }
    }
}
