namespace WebApplication1.ViewModels
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PriceAtTime { get; set; }
        public int Quantity { get; set; }
    }
}