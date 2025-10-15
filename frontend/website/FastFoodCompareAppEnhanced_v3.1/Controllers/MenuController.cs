using Microsoft.AspNetCore.Mvc;
using FastFoodCompareAppEnhanced_v3_1.Data;
using Microsoft.EntityFrameworkCore;

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _db;
        public MenuController(AppDbContext db) => _db = db;

        // Action này bây giờ rất đơn giản
        public IActionResult Index()
        {
            // Không tính toán ViewBag, không lấy dữ liệu từ database nữa.
            // Chỉ trả về View để JavaScript tự xử lý.
            return View();
        }

        // Giữ nguyên action Compare
        public async Task<IActionResult> Compare(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return RedirectToAction("Index");
            var idList = ids.Split(',').Select(s => long.TryParse(s.Trim(), out var x) ? x : 0).Where(x => x > 0).ToList();
            var dishes = await _db.Dishes.Where(d => idList.Contains(d.Id)).ToListAsync();
            return View(dishes);
        }
    }
}