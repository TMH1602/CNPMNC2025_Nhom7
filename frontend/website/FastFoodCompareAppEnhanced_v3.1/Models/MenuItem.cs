using System.Text.Json.Serialization;

namespace FastFoodCompareAppEnhanced_v3_1.Models
{
    public class MenuItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } // ⭐ Thuộc tính cần đếm ⭐

        // Có thể bỏ qua hai thuộc tính này nếu bạn không dùng chúng trong component
        [JsonPropertyName("cartItems")]
        public object[] CartItems { get; set; }

        [JsonPropertyName("orderDetails")]
        public object[] OrderDetails { get; set; }
    }
}