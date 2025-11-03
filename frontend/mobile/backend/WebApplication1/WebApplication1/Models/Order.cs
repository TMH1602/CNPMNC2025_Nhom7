using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class Order
    {
        // Khóa chính
        public int Id { get; set; }

        // Khóa ngoại đến User (Liên kết bằng Username)
        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Username { get; set; } = string.Empty;

        // Thông tin chung
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái đơn hàng

        // Thuộc tính điều hướng
        public User User { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}