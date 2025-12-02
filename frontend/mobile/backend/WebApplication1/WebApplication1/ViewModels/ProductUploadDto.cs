using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    // DTO được thiết kế để nhận dữ liệu FORM (multipart/form-data)
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

        // 💡 Trường nhận File thực tế (Image)
        // Đây là cách bạn nhận tệp được tải lên qua HTTP
        public IFormFile? ImageFile { get; set; }
    }
}