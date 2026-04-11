namespace backend.DTO
{
    public class ImageLinkRequest
    {
        public string Url { get; set; } = null!;
        public bool IsCover { get; set; }
        public string EntityType { get; set; } = null!;
        public int EntityId { get; set; }
    }
}
