namespace backend.DTO
{
    public class ChangeRoleRequest
    {
        public string Role { get; set; } = null!; // Bắt buộc truyền lên (VD: "Admin", "Owner", "User")
    }
}
