namespace backend.DTO
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? UserAvt { get; set; }
        public int Star { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
