using Microsoft.AspNetCore.Mvc.RazorPages;

// Đảm bảo namespace này khớp với dự án của bạn
namespace FastFoodCompareAppEnhanced_v3_1.Areas.Restaurant.Pages
{
    public class KitchenModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Đơn hàng";
        }
    }
}