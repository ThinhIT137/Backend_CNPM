namespace backend.DTO
{
    public class TourismProductDetailRequest
    {
        public int id { get; set; }
        public string type { get; set; }
        public TourismProductRequest TourismProduct { get; set; }
    }
}
