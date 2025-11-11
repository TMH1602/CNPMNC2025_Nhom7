using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// 💡 Controller này CHỈ DÙNG ĐỂ TRẢ VỀ CÁC TRANG HTML
// (Tên namespace của bạn có thể khác)
namespace FastFoodCompareAppEnhanced_v3_1.Controllers 
{
    public class AccountController : Controller
    {
        // === HÀM XỬ LÝ LỖI (401/403/404) ===
        // (Được gọi bởi UseStatusCodePagesWithReExecute trong Program.cs)

        [AllowAnonymous]
        [Route("/Account/HandleError")] // Khớp với tên trong Program.cs
        public IActionResult HandleError(int code)
        {
            if (code == 401) // Lỗi 401 (Chưa đăng nhập)
            {
                // Chuyển hướng đến trang Đăng nhập
                return RedirectToAction("Login", "Account");
            }
            if (code == 403) // Lỗi 403 (Đã đăng nhập nhưng sai vai trò)
            {
                // Chuyển hướng đến trang "Bị cấm" (AccessDenied)
                return RedirectToAction("AccessDenied", "Account");
            }

            // Xử lý các lỗi khác (ví dụ 404)
            ViewData["StatusCode"] = code;
            // (Giả sử bạn có một trang Views/Shared/Error.cshtml mặc định)
            return View("~/Views/Shared/Error.cshtml"); 
        }


        // === TRANG BỊ CẤM (403) ===
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            // Trả về trang Views/Account/AccessDenied.cshtml
            return View(); 
        }

        // === CÁC TRANG CÒN LẠI ===

        // Trả về trang Views/Account/Login.cshtml
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Restaurant()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterRestaurant()
        {
            return View();
        }

        // Trả về trang Views/Account/Register.cshtml
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Trả về trang Views/Account/Profile.cshtml (cho trang cá nhân)
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }
    }
}