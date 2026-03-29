using backend.Models;

namespace backend.DTO
{
    public class TouristAreaDetailResponse
    {
        public Tourist_Area tourist_Area_Detail { get; set; }
        public PagedResult<Tourist_Place> TouristPlaces { get; set; }
        public PagedResult<Tour> Tours { get; set; }
    }
}
