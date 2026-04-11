namespace backend.DTO
{
    public class ReviewRequest
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; } = null!; // "hotel" hoặc "tour"
        public int Star { get; set; } // 1 - 5
        public string? Content { get; set; }
    }
}
