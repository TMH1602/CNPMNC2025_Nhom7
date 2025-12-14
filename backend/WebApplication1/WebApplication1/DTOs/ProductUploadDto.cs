using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ProductUploadDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        public string? Description { get; set; }
        public string? Category { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}