using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class TourItineraryRequest
    {
        [Required] public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Tourist_Place_Id { get; set; }
        public int? DayNumber { get; set; }
    }
}
