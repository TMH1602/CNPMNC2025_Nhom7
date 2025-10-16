using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels; // <-- Rất quan trọng để tránh lỗi JSON Cycle
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ************************************************************
    // 1. Endpoint: GET /api/Cart/{username}
    // Trả về CartDto (ViewModel)
    // ************************************************************
    [HttpGet("{username}")]
    public async Task<ActionResult<CartDto>> GetCart(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Username == username);

        if (cart == null)
        {
            return NotFound($"Cart for user {username} not found.");
        }

        // Ánh xạ từ Model sang CartDto/ViewModel
        var cartViewModel = new CartDto
        {
            Id = cart.Id,
            Username = cart.Username,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Price = ci.Product.Price,
                Quantity = ci.Quantity
            }).ToList()
        };

        return Ok(cartViewModel);
    }

    // ************************************************************
    // 2. Endpoint: POST /api/Cart/add
    // Thêm NHIỀU sản phẩm vào giỏ hàng (Chấp nhận AddItemsToCartDto)
    // ************************************************************
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddItemsToCartDto request)
    {
        // 1. Kiểm tra tính hợp lệ cơ bản
        if (string.IsNullOrEmpty(request.Username) || !request.Items.Any() || request.Items.Any(i => i.Quantity <= 0))
        {
            return BadRequest("Invalid request: Username is required and at least one item with positive quantity must be provided.");
        }

        // 2. Kiểm tra User
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return NotFound($"User '{request.Username}' not found.");

        // 3. Tìm hoặc tạo Cart
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Username == request.Username);

        if (cart == null)
        {
            cart = new Cart { Username = request.Username, User = user };
            _context.Carts.Add(cart);
            // Không cần SaveChangesAsync ở đây, vì sẽ làm cùng lúc cuối cùng
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();

        // Tải tất cả Product cần thiết một lần
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var addedItems = new List<CartAdditionItemDto>();
        var missingProducts = new List<int>();

        // 4. Lặp qua từng sản phẩm trong Request và cập nhật Cart
        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
            {
                missingProducts.Add(item.ProductId);
                continue;
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += item.Quantity; // Cộng thêm số lượng
            }

            // Lưu thông tin item đã được thêm vào (hoặc cập nhật)
            addedItems.Add(new CartAdditionItemDto { ProductId = item.ProductId, Quantity = cartItem.Quantity });
        }

        if (missingProducts.Any())
        {
            return BadRequest($"One or more products were not found: {string.Join(", ", missingProducts)}");
        }

        if (!addedItems.Any())
        {
            return BadRequest("No valid items were provided to add to the cart.");
        }

        await _context.SaveChangesAsync();

        // 5. Trả về Anonymous Object an toàn (Không gây JSON cycle)
        return Ok(new
        {
            Username = request.Username,
            Message = "Multiple products added/updated successfully.",
            ItemsInCart = addedItems.Select(i => new { i.ProductId, CurrentQuantity = i.Quantity })
        });
    }

    // ************************************************************
    // 3. Endpoint: POST /api/Cart/checkout
    // Trả về Anonymous Object (Làm phẳng)
    // ************************************************************
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Username == username);

        if (cart == null || !cart.CartItems.Any())
        {
            return BadRequest("Cart is empty or not found.");
        }

        // 1. Tạo Order mới
        var newOrder = new Order
        {
            Username = username,
            OrderDate = DateTime.UtcNow,
            Status = "Processed",
        };

        decimal totalAmount = 0;
        var orderDetails = new List<OrderDetail>();

        // 2. Chuyển CartItems thành OrderDetails
        foreach (var item in cart.CartItems)
        {
            var detail = new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceAtTime = item.Product.Price
            };
            orderDetails.Add(detail);
            totalAmount += detail.PriceAtTime * detail.Quantity;
        }

        newOrder.TotalAmount = totalAmount;
        newOrder.OrderDetails = orderDetails;

        _context.Orders.Add(newOrder);

        // 3. Xóa các mục trong giỏ hàng (CartItems)
        _context.CartItems.RemoveRange(cart.CartItems);

        await _context.SaveChangesAsync();

        // 4. Trả về Anonymous Object an toàn
        return CreatedAtAction(nameof(GetOrderHistory), new { username = username }, new
        {
            OrderId = newOrder.Id,
            newOrder.TotalAmount,
            newOrder.OrderDate,
            Message = "Checkout successful."
        });
    }

    // ************************************************************
    // 4. Endpoint: GET /api/Cart/history/{username}
    // Trả về List<OrderHistoryDto> (ViewModel)
    // ************************************************************
    [HttpGet("history/{username}")]
    public async Task<ActionResult<IEnumerable<OrderHistoryDto>>> GetOrderHistory(string username)
    {
        var orders = await _context.Orders
            .Where(o => o.Username == username)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        if (!orders.Any())
        {
            return NotFound("No order history found.");
        }

        // Ánh xạ sang OrderHistoryDto/ViewModel
        var historyViewModels = orders.Select(o => new OrderHistoryDto
        {
            OrderId = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Items = o.OrderDetails.Select(od => new OrderItemDto
            {
                ProductId = od.ProductId,
                ProductName = od.Product?.Name ?? "N/A", // Xử lý trường hợp Product là null
                Quantity = od.Quantity,
                PriceAtTime = od.PriceAtTime
            }).ToList()
        }).ToList();

        return Ok(historyViewModels);
    }
}