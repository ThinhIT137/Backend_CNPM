namespace backend.DTO
{
    public class AdRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Position { get; set; } = "Home"; // VD: Home, Sidebar
        public string Url { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
