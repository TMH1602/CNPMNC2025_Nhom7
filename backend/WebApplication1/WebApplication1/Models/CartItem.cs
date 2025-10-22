using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // <--- DÒNG NÀY PHẢI CÓ CHO [PrimaryKey]

namespace WebApplication1.Models
{
    // [PrimaryKey] được sử dụng để định nghĩa Khóa chính Kép (Composite Key)
    // Nếu lỗi vẫn xảy ra, hãy xoá dòng này và chỉ dùng HasKey trong DbContext.
    public class CartItem
    {
        // Khóa ngoại (ForeignKey) đến bảng Cart
        [Required]
        public int CartId { get; set; }

        // Khóa ngoại (ForeignKey) đến bảng Product
        [Required]
        public int ProductId { get; set; }

        // Trường đặc trưng của mối quan hệ N-N (ví dụ: Số lượng)
        [Required]
        public int Quantity { get; set; }

        // Thuộc tính Điều hướng (Navigation Properties)
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public string ImageUrl { get; internal set; }
    }
}