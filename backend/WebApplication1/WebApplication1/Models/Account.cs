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
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        [MaxLength(250)] // Giới hạn độ dài chuỗi địa chỉ
        public string Address { get; set; }

        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
