using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tourist_Area
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public Guid? Created_By_UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RatingTotal { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public decimal RatingAverage { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;

        // FK
        [ForeignKey("Created_By_UserId")]
        public virtual User? User { get; set; }

        // điều hướng 1 - N
        public virtual ICollection<Tourist_Place> Tourist_Places { get; set; } = new List<Tourist_Place>();
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
