using Microsoft.AspNetCore.Mvc;

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    public class MobilePaymentController : Controller
    {
        // Action này chỉ để hiển thị trang form thanh toán
        public IActionResult MobilePayment()
        {
            return View();
        }

        // Action này để hiển thị trang báo thành công
        public IActionResult Success()
        {
            return View();
        }
    }
}