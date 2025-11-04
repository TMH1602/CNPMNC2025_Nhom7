using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    // DTO này chỉ chứa các trường cần thiết để TẠO MỚI một Product
    public class ProductCreationDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
        [Range(0.01, 10000.00, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public string? ImageUrl { get; set; }
    }
}