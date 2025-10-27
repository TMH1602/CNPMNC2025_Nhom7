using Microsoft.AspNetCore.Mvc;

// Controller này CHỈ DÙNG ĐỂ TRẢ VỀ CÁC TRANG HTML
public class AccountController : Controller
{
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