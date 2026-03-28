using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Img
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? url { get; set; }
        public bool IsCover { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
