using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FastFoodCompareAppEnhanced_v3_1.Areas.Admin.Pages
{
    // Giả sử bạn cũng yêu cầu đăng nhập cho trang này
    // [Authorize(Roles = "Admin")] 
    public class RevenueModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Báo cáo Doanh thu";
        }
    }
}