using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Đã thay đổi internal thành public để DbContext có thể truy cập
    public class ProductDeletedByAdmin
    {
        // ID và OriginalProductId là int (non-nullable) và được gán giá trị, nên ổn.
        [Required]
        public int Id { get; set; } // ID của bản ghi lưu trữ
        [Required]
        public int OriginalProductId { get; set; } // ID gốc của Product

        // Khắc phục: Loại bỏ dấu '?' và gán giá trị mặc định cho các thuộc tính bắt buộc
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; } // decimal là non-nullable và được gán giá trị

        // Khắc phục: Loại bỏ dấu '?' và gán giá trị mặc định
        [Required]
        public string Description { get; set; } = string.Empty;

        // Khắc phục: Loại bỏ dấu '?' và gán giá trị mặc định
        [Required]
        public string Category { get; set; } = string.Empty;

        // Khắc phục: Loại bỏ dấu '?' và gán giá trị mặc định
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;

        // DateTime là non-nullable và được gán giá trị (hoặc lấy giá trị mặc định của hệ thống)
        public DateTime DeletedDate { get; set; }
    }
}