using Microsoft.AspNetCore.Mvc;
using FastFoodCompareAppEnhanced_v3_1.Models; // Quan trọng: Phải using Model

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    [Route("api/Menu")] // Đây chính là URL: https://localhost:xxxx/api/Menu
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        [HttpGet] // Phương thức này sẽ được gọi khi có yêu cầu GET
        public ActionResult<IEnumerable<MenuItem>> GetMenu()
        {
            // Dữ liệu thực đơn của bạn
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Id = 1, Name = "Gà Rán Giòn Tan (1M)", Price = 59000, Description = "Gà rán giòn tan, chuẩn vị. (1 Miếng)", Category = "Gà" },
                new MenuItem { Id = 2, Name = "Combo Gà Vui Vẻ", Price = 99000, Description = "2 Gà rán + 1 Khoai tây chiên cỡ vừa + 1 Nước ngọt.", Category = "Combo" },
                new MenuItem { Id = 3, Name = "Burger Bò Phô Mai", Price = 75000, Description = "Thịt bò Úc, phô mai tan chảy, sốt đặc biệt.", Category = "Burger" },
                // ... dán tất cả 20 món ăn của bạn vào đây ...
                new MenuItem { Id = 20, Name = "Phở Bò Tái", Price = 65000, Description = "Phở truyền thống, thịt bò tái.", Category = "Khác" }
            };

            return Ok(menuItems); // Trả về dữ liệu dưới dạng JSON với mã 200 OK
        }
    }
}