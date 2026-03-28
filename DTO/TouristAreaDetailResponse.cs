using backend.Models;

namespace backend.DTO
{
    public class TouristAreaDetailResponse
    {
        public Tourist_Area Tourist_Area_Detail { get; set; }
        public PagedResult<Tourist_Place> PagedResult { get; set; }
    }
}
