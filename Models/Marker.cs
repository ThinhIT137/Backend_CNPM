using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Marker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guid CreatedByUserId { get; set; }
        public int? TouristPlaceId { get; set; }

        //Fk
        [ForeignKey("CreatedByUserId")]
        public virtual User? User { get; set; }
        [ForeignKey("TouristPlaceId")]
        public virtual Tourist_Place? Tourist_Place { get; set; }
    }
}
