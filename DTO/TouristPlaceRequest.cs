namespace backend.DTO
{
    public class TouristPlaceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // ĐỔI SANG DECIMAL ĐỂ ĐỒNG BỘ VỚI MODEL
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public int Tourist_Area_Id { get; set; }
    }
}