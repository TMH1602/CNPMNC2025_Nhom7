namespace WebApplication1.ViewModels
{
    public class CartAdditionItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddItemsToCartDto
    {
        public string Username { get; set; } = string.Empty;

        public List<CartAdditionItemDto> Items { get; set; } = new List<CartAdditionItemDto>();
    }
}