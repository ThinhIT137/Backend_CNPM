namespace backend.DTO
{
    public class TouristAreaDetailRequest
    {
        public int id { get; set; }
        public string type { get; set; }
        public TourismProductRequest TourismProduct { get; set; }
    }
}
