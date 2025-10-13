namespace FastFoodCompareApp.Models
{
    public class Dish
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Calories { get; set; }
        public decimal Rating { get; set; } // 0-5
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
