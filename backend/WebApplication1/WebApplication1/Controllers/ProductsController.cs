using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models; 
using WebApplication1.Data; 
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.ViewModels;
using WebApplication1.Services;
using System.Linq;

namespace MyFastFoodApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class MenuController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService; 
        public MenuController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenu()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        
        [HttpGet("{id}")] 
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} trong thực đơn. ❌"); // HTTP 404 Not Found
            }

            return Ok(product);
        }
        [HttpPost] 
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProduct([FromForm] ProductUploadDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string? imageUrl = null;
            if (productDto.ImageFile != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ImageFile);

                if (imageUrl == null)
                {
                    return BadRequest("Image upload failed. Please check the image file or server logs.");
                }
            }
            var newProduct = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description,
                Category = productDto.Category,
                ImageUrl = imageUrl,
                IsActive = true
            };
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreationDto updatedProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} để cập nhật. ❌"); // HTTP 404 Not Found
            }

            existingProduct.Name = updatedProductDto.Name;
            existingProduct.Price = updatedProductDto.Price;
            existingProduct.Description = updatedProductDto.Description;
            existingProduct.Category = updatedProductDto.Category;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {

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
                _context.ProductDeletedByAdmins.Add(deletedRecord);
                var existingProduct = await _context.Products.FindAsync(id);
                existingProduct.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok($"Món ăn ID {id} đã được đánh dấu không hoạt động để bảo toàn lịch sử.");
            }
            else
            {
                _context.Products.Remove(productToDelete1);

                await _context.SaveChangesAsync();

                return Ok($"Món ăn ID {id} đã được xóa thành công!");
            }
        }
    }
}