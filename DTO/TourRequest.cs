using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class TourRequest
    {
        [Required] public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int? DurationDays { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? DepartureLocationName { get; set; }
        public decimal? DepartureLatitude { get; set; }
        public decimal? DepartureLongitude { get; set; }
        public string? Vehicle { get; set; }
        public string? TourType { get; set; }
        public int Tourist_Area_Id { get; set; }
        public decimal? Price { get; set; }
    }
}
