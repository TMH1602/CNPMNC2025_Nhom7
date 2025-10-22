using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")] 
        public string Username { get; set; } = string.Empty;
        public bool IsProcessed { get; set; } = false;
        public User User { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}