namespace FastFoodChain.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Lưu trữ mật khẩu đã hash
        public string Name { get; set; } = string.Empty;

        // 1: Admin, 2: Standard User
        public int RoleToken { get; set; } = 2;
    }
}