using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// Model đại diện cho một món ăn/sản phẩm trong cơ sở dữ liệu.
    /// </summary>
    public class Product
    {
        // Khóa chính (Primary Key) và tự động tăng
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Dùng decimal(18, 2) cho trường tiền tệ
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(300)] 
        public string? ImageUrl { get; set; }
    }
}
