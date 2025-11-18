using Microsoft.AspNetCore.Authorization; // <-- 1. Thêm dòng này
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FastFoodCompareAppEnhanced_v3_1.Areas.Admin.Pages.Products
{
    [Authorize(Policy = "AdminOnly")] // <-- 2. Thêm dòng này
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}