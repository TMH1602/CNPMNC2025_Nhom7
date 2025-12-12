using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class AllCartsDto
    {
        public int CartId { get; set; }
        public string Username { get; set; } = string.Empty;

        public int TotalItems { get; set; }

        public int TotalQuantity { get; set; }
        public bool IsProcessed { get; set; }

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}