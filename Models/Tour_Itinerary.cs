using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tour_Itinerary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int TourId { get; set; }
        public int Tourist_Place_Id { get; set; }
        public int? DayNumber { get; set; }

        // Fk
        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }
        [ForeignKey("Tourist_Place_Id")]
        public virtual Tourist_Place? Tourist_Place { get; set; }
    }
}
