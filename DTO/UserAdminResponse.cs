namespace backend.DTO
{
    public class UserAdminResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Avt { get; set; } // Thích thì nhét luôn Avt vào cho Admin ngắm
        public string Email { get; set; } = null!;
        public string? Role { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
