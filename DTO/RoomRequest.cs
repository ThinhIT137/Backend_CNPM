namespace backend.DTO
{
    public class RoomRequest
    {
        public string RoomName { get; set; } = null!;
        public int Floor { get; set; }
        public string RoomType { get; set; } = "Standard";
        public decimal Price { get; set; }
    }
}
