using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class CartDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}