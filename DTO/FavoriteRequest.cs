namespace backend.DTO
{
    public class FavoriteRequest
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; } = null!; // "hotel", "tour", "tourist_area"
    }
}
