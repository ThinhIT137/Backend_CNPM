using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tourist_Place
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
        public string? Status { get; set; } // Active, Approved, Rejected
        public Guid? Created_By_UserId { get; set; }
        public int Tourist_Area_Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RatingTotal { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public decimal RatingAverage { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;

        // FK
        [ForeignKey("Created_By_UserId")]
        public virtual User? User { get; set; }
        [ForeignKey("Tourist_Area_Id")]
        public virtual Tourist_Area? Tourist_Area { get; set; }


        // điều hướng 1 - N
        public virtual ICollection<Tour_Itinerary> Tour_Itineraries { get; set; } = new List<Tour_Itinerary>();
        public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
        public virtual ICollection<Marker> Markers { get; set; } = new List<Marker>();

    }
}
