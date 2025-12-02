using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FastFoodCompareAppEnhanced_v3_1.Areas.Admin.Pages.Products
{
    [Authorize(Policy = "AdminOnly")] // Vẫn bảo vệ trang như bình thường
    public class IndexBlazorModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}