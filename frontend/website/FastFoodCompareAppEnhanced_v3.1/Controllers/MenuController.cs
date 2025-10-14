using Microsoft.AspNetCore.Mvc;
using FastFoodCompareAppEnhanced_v3_1.Data;
using Microsoft.EntityFrameworkCore;

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _db;
        public MenuController(AppDbContext db) => _db = db;

        // Shows all dishes and comparison summary
        public async Task<IActionResult> Index()
        {
            var dishes = await _db.Dishes.ToListAsync();

            // comparison metrics
            ViewBag.AvgPrice = dishes.Any() ? dishes.Average(d => (double)d.Price) : 0;
            ViewBag.AvgCalories = dishes.Any() ? dishes.Average(d => d.Calories) : 0;
            ViewBag.BestValue = dishes.OrderBy(d => d.Price / (d.Calories == 0 ? 1 : d.Calories)).FirstOrDefault();
            ViewBag.HighestRating = dishes.OrderByDescending(d => d.Rating).FirstOrDefault();

            return View(dishes);
        }

        // Show details for selected dishes (ids comma-separated)
        public async Task<IActionResult> Compare(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return RedirectToAction("Index");
            var idList = ids.Split(',').Select(s => long.TryParse(s.Trim(), out var x) ? x : 0).Where(x => x>0).ToList();
            var dishes = await _db.Dishes.Where(d => idList.Contains(d.Id)).ToListAsync();
            return View(dishes);
        }
    }
}
