namespace WebApplication1.ViewModels
{
    // DTO/ViewModel cho từng mục sản phẩm trong giỏ hàng
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        // Trường tính toán (tùy chọn)
        public decimal TotalItemPrice => Price * Quantity;
    }
}