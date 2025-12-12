using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{username}")]
    [Authorize]
    public async Task<ActionResult<CartDto>> GetCart(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Username == username && c.IsProcessed == false);

        if (cart == null)
        {
            return NotFound($"Active cart for user {username} not found.");
        }
        var productId = cart.CartItems.FirstOrDefault()?.ProductId;

        if (productId.HasValue)
        {
            var existingProduct = await _context.Products.FindAsync(productId.Value);
        }
        else
        {
            return BadRequest("Không thể tìm được giỏ hàng của người dùng ");
        }
        var cartViewModel = new CartDto
        {
            Id = cart.Id,
            Username = cart.Username,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Price = ci.Product.Price,
                ImageUrl = ci.Product.ImageUrl,
                Quantity = ci.Quantity
            }).ToList()
        };

        return Ok(cartViewModel);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddItemsToCartDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || !request.Items.Any() || request.Items.Any(i => i.Quantity <= 0))
        {
            return BadRequest("Invalid request: Username is required and at least one item with positive quantity must be provided.");
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return NotFound($"User '{request.Username}' not found.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Username == request.Username && c.IsProcessed == false);

        if (cart == null)
        {
            cart = new Cart { Username = request.Username, User = user, IsProcessed = false };
            _context.Carts.Add(cart);
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var addedItems = new List<CartAdditionItemDto>();
        var missingProducts = new List<int>();

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
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
                    Quantity = item.Quantity,
                    ImageUrl = product.ImageUrl
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += item.Quantity;
            }

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

        return Ok(new
        {
            Username = request.Username,
            Message = "Multiple products added/updated successfully.",
            ItemsInCart = addedItems.Select(i => new { i.ProductId, CurrentQuantity = i.Quantity })
        });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromCart(string username, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return BadRequest("Quantity must be positive for removal.");
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Username == username && c.IsProcessed == false);

        if (cart == null)
        {
            return NotFound($"Active cart for user {username} not found.");
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem == null)
        {
            return NotFound($"Product ID {productId} not found in user's cart.");
        }

        string message;

        if (cartItem.Quantity <= quantity)
        {
            _context.CartItems.Remove(cartItem);
            message = $"Product ID {productId} has been completely removed from the cart.";
        }
        else
        {
            cartItem.Quantity -= quantity;
            message = $"Removed {quantity} units of Product ID {productId}. New quantity: {cartItem.Quantity}.";
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Username = username,
            ProductId = productId,
            CurrentQuantity = cartItem.Quantity,
            Message = message
        });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Username == username && c.IsProcessed == false);

        if (cart == null || !cart.CartItems.Any())
        {
            return BadRequest("Active cart is empty or not found.");
        }

        var newOrder = new Order
        {
            Username = username,
            OrderDate = DateTime.UtcNow,
            Status = "Processed",
        };

        decimal totalAmount = 0;
        var orderDetails = new List<OrderDetail>();

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

        cart.IsProcessed = true;


        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderHistory), new { username = username }, new
        {
            OrderId = newOrder.Id,
            newOrder.TotalAmount,
            newOrder.OrderDate,
            Message = "Checkout successful."
        });
    }

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

        var historyViewModels = orders.Select(o => new OrderHistoryDto
        {
            OrderId = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Items = o.OrderDetails.Select(od => new OrderItemDto
            {
                ProductId = od.ProductId,
                ProductName = od.Product?.Name ?? "N/A",
                Quantity = od.Quantity,
                PriceAtTime = od.PriceAtTime
            }).ToList()
        }).ToList();

        return Ok(historyViewModels);
    }

    [HttpGet("AllCarts")]
    public async Task<ActionResult<IEnumerable<AllCartsDto>>> GetAllCarts()
    {
        var allCarts = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .ToListAsync();

        if (!allCarts.Any())
        {
            return NotFound("No carts found in the system.");
        }

        var allCartsViewModel = allCarts.Select(cart => new AllCartsDto
        {
            CartId = cart.Id,
            Username = cart.Username,
            TotalItems = cart.CartItems.Count,
            TotalQuantity = cart.CartItems.Sum(ci => ci.Quantity),

            IsProcessed = cart.IsProcessed,

            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "N/A",
                Price = ci.Product?.Price ?? 0,
                Quantity = ci.Quantity
            }).ToList()
        }).ToList();

        return Ok(allCartsViewModel);
    }
}