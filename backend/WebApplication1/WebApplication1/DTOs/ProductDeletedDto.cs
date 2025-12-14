using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ProductDeletedDto
    {
        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
