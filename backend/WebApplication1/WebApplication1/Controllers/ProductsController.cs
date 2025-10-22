using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models; // Đảm bảo bạn tham chiếu đúng namespace của Product Model
using WebApplication1.Data; // Thêm tham chiếu đến ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Cần thiết cho các phương thức Async (ToListAsync, FindAsync)
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.ViewModels; // Thêm DTO/ViewModels
using WebApplication1.Services; // Thêm Cloudinary Service
using System.Linq; // Cần cho ToList()

namespace MyFastFoodApi.Controllers
{
    /// <summary>
    /// Controller dùng để quản lý các món ăn/sản phẩm trong thực đơn, sử dụng Entity Framework Core.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Route: /api/Menu
    public class MenuController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService; // Inject Service

        // 1. Dependency Injection: Nhận cả Context và Service
        public MenuController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // ====================================================================
        // ENDPOINT: ĐỌC TẤT CẢ (GET)
        // ====================================================================

        [HttpGet] // Route: /api/Menu
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenu()
        {
            // Đọc toàn bộ dữ liệu từ bảng Products trong Database
            var products = await _context.Products.ToListAsync();
            return Ok(products); // HTTP 200 OK
        }

        // ====================================================================
        // ENDPOINT: ĐỌC THEO ID (GET {id})
        // ====================================================================

        /// <param name="id">ID của món ăn (ví dụ: 1).</param>
        /// <returns>Thông tin Product hoặc HTTP 404 Not Found.</returns>
        [HttpGet("{id}")] // Route: /api/Menu/{id}
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            // FindAsync chỉ tìm kiếm theo Khóa chính (Primary Key)
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} trong thực đơn. ❌"); // HTTP 404 Not Found
            }

            return Ok(product); // HTTP 200 OK
        }

        // ====================================================================
        // ENDPOINT: THÊM MỚI (POST) - SỬ DỤNG ProductUploadDto & Cloudinary Service
        // ====================================================================

        /// <param name="productDto">DTO chứa dữ liệu Product và ImageFile (multipart/form-data).</param>
        /// <returns>Món ăn vừa được tạo cùng với ID mới và HTTP 201 Created.</returns>
        [HttpPost] // Route: /api/Menu
        [Consumes("multipart/form-data")] // Chấp nhận dữ liệu form có tệp
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProduct([FromForm] ProductUploadDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? imageUrl = null;

            // 1. Tải tệp lên Cloudinary (Sử dụng Service)
            if (productDto.ImageFile != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ImageFile);

                if (imageUrl == null)
                {
                    // Service trả về null nếu có lỗi tải lên
                    return BadRequest("Image upload failed. Please check the image file or server logs.");
                }
            }

            // 2. Ánh xạ (Mapping) từ DTO sang Model Entity Framework
            var newProduct = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description,
                Category = productDto.Category,
                ImageUrl = imageUrl, // <-- URL công khai được lưu vào DB
                IsActive = true
            };

            // 3. Thêm và lưu vào Database
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            // Trả về HTTP 201 Created 
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        // ====================================================================
        // ENDPOINT: CẬP NHẬT (PUT) - SỬ DỤNG ProductCreationDto
        // Lưu ý: Endpoint này không hỗ trợ upload file, chỉ cập nhật URL
        // Nếu muốn update file, cần tạo 1 PUT/PATCH riêng với IFormFile
        // ====================================================================

        /// <param name="id">ID của món ăn cần sửa (ví dụ: 1).</param>
        /// <param name="updatedProductDto">DTO với thông tin mới (bao gồm ImageUrl, không IFormFile).</param>
        /// <returns>HTTP 204 No Content nếu thành công hoặc HTTP 404 Not Found.</returns>
        [HttpPut("{id}")] // Route: /api/Menu/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        // 💡 SỬA: Nhận ProductCreationDto (DTO không có IFormFile)
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreationDto updatedProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400 Bad Request
            }

            // 1. Tìm Entity đang được theo dõi
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} để cập nhật. ❌"); // HTTP 404 Not Found
            }

            // 2. Cập nhật thông tin từ DTO lên Entity đang được theo dõi
            existingProduct.Name = updatedProductDto.Name;
            existingProduct.Price = updatedProductDto.Price;
            existingProduct.Description = updatedProductDto.Description;
            existingProduct.Category = updatedProductDto.Category;
            // *** Cập nhật ImageUrl (chỉ là string, không phải upload file) ***
            existingProduct.ImageUrl = updatedProductDto.ImageUrl;

            // 3. Lưu thay đổi vào Database
            await _context.SaveChangesAsync();

            // 4. Trả về HTTP 204 No Content
            return NoContent();
        }

        // ====================================================================
        // ENDPOINT: XÓA (DELETE)
        // ====================================================================

        /// <param name="id">ID của món ăn cần xóa (ví dụ: 1).</param>
        /// <returns>HTTP 204 No Content nếu thành công hoặc HTTP 404 Not Found.</returns>
        [HttpDelete("{id}")] // Route: /api/Menu/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // 1. Tìm món ăn cần xóa
            var productToDelete1 = await _context.Products.FindAsync(id);

            if (productToDelete1 == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} để xóa"); // HTTP 404 Not Found
            }

            bool hasBeenOrdered = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            var relatedCartItems = await _context.CartItems
                .Where(ci => ci.ProductId == id)
                .ToListAsync();
            if (relatedCartItems.Any())
            {
                _context.CartItems.RemoveRange(relatedCartItems);
            }

            if (hasBeenOrdered)
            {
                var deletedRecord = new ProductDeletedByAdmin
                {
                    OriginalProductId = productToDelete1.Id,
                    Name = productToDelete1.Name,
                    Price = productToDelete1.Price,
                    Description = productToDelete1.Description,
                    Category = productToDelete1.Category,
                    ImageUrl = productToDelete1.ImageUrl,
                    IsActive = false,
                    DeletedDate = DateTime.UtcNow
                };
                _context.ProductDeletedByAdmin.Add(deletedRecord);
                var existingProduct = await _context.Products.FindAsync(id);
                existingProduct.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok($"Món ăn ID {id} đã được đánh dấu không hoạt động để bảo toàn lịch sử.");
            }
            else
            {
                _context.Products.Remove(productToDelete1);

                // Lưu thay đổi vào Database
                await _context.SaveChangesAsync();

                // 4. Trả về HTTP 204 No Content
                return Ok($"Món ăn ID {id} đã được xóa thành công!");
            }
        }
    }
}