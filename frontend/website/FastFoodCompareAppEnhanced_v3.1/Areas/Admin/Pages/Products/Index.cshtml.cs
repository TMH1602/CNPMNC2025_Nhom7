using Microsoft.AspNetCore.Mvc.RazorPages;

// Namespace phải phản ánh chính xác cấu trúc thư mục của Razor Pages Area
namespace FastFoodCompareAppEnhanced_v3_1.Areas.Admin.Pages.Products
{
    // Kế thừa từ PageModel
    public class IndexModel : PageModel
    {
        // Bạn có thể thêm các Dependencies (như HttpClient) vào đây
        // public IndexModel(IHttpClientFactory httpClientFactory) { ... }

        // Handler OnGet sẽ được gọi khi bạn truy cập trang (GET request)
        public void OnGet()
        {
            // Trong Admin Area, ta thường để trống phần này vì JavaScript/Fetch API 
            // sẽ tự xử lý việc tải dữ liệu sau khi trang HTML được render.
        }
        
        // Các handler khác: OnPostCreate, OnPostDelete, OnGetEdit...
        // có thể được thêm vào để xử lý form submit bằng C# nếu cần.
    }
}