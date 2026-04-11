namespace backend.DTO
{
    public class MarkerRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsPublic { get; set; }
        public int Tourist_Place_Id { get; set; }
    }
}
