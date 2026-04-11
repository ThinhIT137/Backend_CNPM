using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Advertisement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Position { get; set; } // home, detail....
        public string? Size { get; set; } // dai, rong, ...
        public string? Url { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? Start_date { get; set; } = DateTime.Now;
        public DateTime? End_date { get; set; } = DateTime.Now.AddDays(31);
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}
