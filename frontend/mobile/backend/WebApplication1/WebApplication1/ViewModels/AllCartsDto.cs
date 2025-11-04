using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    // Sử dụng CartItemDto đã có

    // ViewModel đại diện cho MỘT giỏ hàng trong danh sách tất cả giỏ hàng
    public class AllCartsDto
    {
        public int CartId { get; set; }
        public string Username { get; set; } = string.Empty;

        // Tổng số loại sản phẩm trong giỏ
        public int TotalItems { get; set; }

        // Tổng số lượng sản phẩm (cộng dồn Quantity)
        public int TotalQuantity { get; set; }
        public bool IsProcessed { get; set; }

        // Danh sách các mục chi tiết trong giỏ
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}