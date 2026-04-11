using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class TouristAreaRequest
    {
        [Required(ErrorMessage = "Tên khu du lịch không được để trống")]
        public string Name { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
    }
}
