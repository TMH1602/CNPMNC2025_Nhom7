namespace FastFoodCompareAppEnhanced_v3_1.Models
{
    public class CartItem
    {
        public long DishId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Total => Price * Quantity;
    }
}
