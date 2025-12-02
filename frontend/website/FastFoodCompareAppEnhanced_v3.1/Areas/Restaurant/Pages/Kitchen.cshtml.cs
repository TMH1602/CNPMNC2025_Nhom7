using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization; // 1. THÊM THƯ VIỆN NÀY

// Đảm bảo namespace này khớp với dự án của bạn
namespace FastFoodCompareAppEnhanced_v3_1.Areas.Restaurant.Pages
{
    [Authorize(Policy = "RestaurantOnly")] // 2. THÊM POLICY NÀY
    public class KitchenModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Đơn hàng";
        }
    }
}