using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models; // Đảm bảo bạn tham chiếu đúng namespace của Product Model
using WebApplication1.Data; // Thêm tham chiếu đến ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Cần thiết cho các phương thức Async (ToListAsync, FindAsync)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyFastFoodApi.Controllers
{
    /// <summary>
    /// Controller dùng để quản lý các món ăn/sản phẩm trong thực đơn, sử dụng Entity Framework Core.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Route: /api/Menu
    public class MenuController : ControllerBase
    {
        // Loại bỏ danh sách giả lập tĩnh và thay bằng DbContext
        private readonly ApplicationDbContext _context;

        // 1. Dependency Injection: Context được inject tự động nhờ cấu hình trong Program.cs
        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // ENDPOINT: ĐỌC TẤT CẢ (GET)
        // ====================================================================

        [HttpGet] // Route: /api/Menu
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenu() // Sử dụng Task<IActionResult> và async/await
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
        public async Task<IActionResult> GetProductById(int id) // Sử dụng async/await
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
        // ENDPOINT: THÊM MỚI (POST)
        // Đã cập nhật để nhận ImageUrl qua JSON body
        // ====================================================================

        /// <param name="newProduct">Đối tượng Product mới cần thêm (bao gồm ImageUrl, không cần Id).</param>
        /// <returns>Món ăn vừa được tạo cùng với ID mới và HTTP 201 Created.</returns>
        [HttpPost] // Route: /api/Menu
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProduct([FromBody] Product newProduct) // Sử dụng async/await
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400 Bad Request
            }

            // 1. Thêm vào DbContext (Chưa lưu vào DB)
            _context.Products.Add(newProduct);

            // 2. Lưu thay đổi vào Database
            await _context.SaveChangesAsync();
            // Sau khi lưu thành công, EF Core tự động gán Id được sinh ra từ DB vào newProduct

            // Trả về HTTP 201 Created 
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        // ====================================================================
        // ENDPOINT: CẬP NHẬT (PUT)
        // Đã cập nhật để xử lý ImageUrl
        // ====================================================================

        /// <param name="id">ID của món ăn cần sửa (ví dụ: 1).</param>
        /// <param name="updatedProduct">Đối tượng Product với thông tin mới (bao gồm ImageUrl).</param>
        /// <returns>HTTP 204 No Content nếu thành công hoặc HTTP 404 Not Found.</returns>
        [HttpPut("{id}")] // Route: /api/Menu/{id}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct) // Sử dụng async/await
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400 Bad Request
            }

            // 1. Tìm Entity đang được theo dõi (hoặc trả về null)
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} để cập nhật. ❌"); // HTTP 404 Not Found
            }

            // 2. Cập nhật thông tin lên Entity đang được theo dõi
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Category = updatedProduct.Category;
            // *** Cập nhật trường ImageUrl ***
            existingProduct.ImageUrl = updatedProduct.ImageUrl;

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
        public async Task<IActionResult> DeleteProduct(int id) // Sử dụng async/await
        {
            // 1. Tìm món ăn cần xóa
            var productToDelete = await _context.Products.FindAsync(id);

            if (productToDelete == null)
            {
                return NotFound($"Không tìm thấy món ăn có ID: {id} để xóa. ❌"); // HTTP 404 Not Found
            }

            // 2. Xóa khỏi DbContext
            _context.Products.Remove(productToDelete);

            // 3. Lưu thay đổi vào Database
            await _context.SaveChangesAsync();

            // 4. Trả về HTTP 204 No Content
            return NoContent();
        }
    }
}