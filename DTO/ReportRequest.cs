namespace backend.DTO
{
    public class ReportRequest
    {
        public string EntityType { get; set; } = null!; // "hotel", "tour", "review"
        public int EntityId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Description { get; set; }
    }
}
