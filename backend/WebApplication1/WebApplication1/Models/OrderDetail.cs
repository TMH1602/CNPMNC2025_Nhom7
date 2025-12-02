using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    // Khóa chính kép: OrderId và ProductId
    public class OrderDetail
    {
        // Khóa ngoại đến Order
        [Required]
        public int OrderId { get; set; }

        // Khóa ngoại đến Product
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        // Lưu giá bán tại thời điểm đặt hàng (để lưu lịch sử)
        public decimal PriceAtTime { get; set; }

        // Thuộc tính điều hướng
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}