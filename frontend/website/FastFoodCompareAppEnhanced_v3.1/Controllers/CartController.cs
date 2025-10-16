using Microsoft.AspNetCore.Mvc;
using FastFoodCompareAppEnhanced_v3_1.Models;
using FastFoodCompareAppEnhanced_v3_1.Data;
using Microsoft.EntityFrameworkCore;
using FastFoodCompareAppEnhanced_v3_1; // <-- ensure extension methods visible

namespace FastFoodCompareAppEnhanced_v3_1.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;
        private const string CART_KEY = "CART_ITEMS";

        public CartController(AppDbContext db)
        {
            _db = db;
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY);
            return cart ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
        }

        public IActionResult Index()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(long id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.DishId == id);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem
                {
                    DishId = dish.Id,
                    Name = dish.Name,
                    Price = dish.Price,
                    ImageUrl = dish.ImageUrl,
                    Quantity = 1
                });

            SaveCart(cart);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(long id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.DishId == id);
            if (item != null)
            {
                if (quantity <= 0) cart.RemoveAll(c => c.DishId == id);
                else item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(long id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.DishId == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Index");
        }
    }
}
