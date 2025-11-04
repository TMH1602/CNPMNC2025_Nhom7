namespace WebApplication1.ViewModels
{
    // Đại diện cho một mục sản phẩm muốn thêm vào giỏ hàng
    public class CartAdditionItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Đại diện cho toàn bộ request body khi thêm nhiều sản phẩm
    public class AddItemsToCartDto
    {
        // Tên người dùng mà giỏ hàng thuộc về
        public string Username { get; set; } = string.Empty;

        // Danh sách các sản phẩm và số lượng tương ứng
        public List<CartAdditionItemDto> Items { get; set; } = new List<CartAdditionItemDto>();
    }
}