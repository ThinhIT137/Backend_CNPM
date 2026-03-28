namespace backend.DTO
{
    public class RegisterRequest
    {
        public string? Name { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
