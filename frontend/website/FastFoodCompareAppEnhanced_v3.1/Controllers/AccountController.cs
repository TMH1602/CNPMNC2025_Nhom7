using Microsoft.AspNetCore.Mvc;
using FastFoodCompareAppEnhanced_v3_1.Data;
using FastFoodCompareAppEnhanced_v3_1.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Menu");
            }
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string fullname)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var exists = await _db.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            var user = new UserAccount
            {
                Username = username,
                Password = password,
                FullName = string.IsNullOrWhiteSpace(fullname) ? username : fullname
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // auto login
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index","Menu");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
