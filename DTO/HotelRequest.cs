using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class HotelRequest
    {
        [Required] public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int? NumberOfPeople { get; set; }
        public int Tourist_Place_Id { get; set; }
        public decimal? Price { get; set; }
    }
}
