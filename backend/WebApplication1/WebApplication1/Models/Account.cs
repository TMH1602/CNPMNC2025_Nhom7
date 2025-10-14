using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// Entity đại diện cho một tài khoản người dùng trong cơ sở dữ liệu.
    /// </summary>
    public class User
    {
        // Khóa chính
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty; // Dùng làm ID đăng nhập

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        // Trong thực tế, đây sẽ là Password HASH, nhưng tôi giữ là string
        // để đơn giản hóa ví dụ này (giống như logic giả lập của bạn).
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
