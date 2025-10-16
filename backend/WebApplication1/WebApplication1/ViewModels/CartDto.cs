using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    // DTO/ViewModel cho Giỏ hàng
    public class CartDto
    {
        public int Id { get; set; }

        // Sử dụng Username để tránh vòng lặp tham chiếu với đối tượng User
        public string Username { get; set; } = string.Empty;

        // Danh sách các mục trong giỏ hàng (sử dụng CartItemDto)
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}