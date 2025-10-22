using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
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
    // Lấy giỏ hàng CHƯA XỬ LÝ (IsProcessed = false)
    // ************************************************************
    [HttpGet("{username}")]
    public async Task<ActionResult<CartDto>> GetCart(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            // 💡 CHỈ LẤY giỏ hàng KHÔNG được xử lý
            .FirstOrDefaultAsync(c => c.Username == username && c.IsProcessed == false);

        if (cart == null)
        {
            // Trả về Not Found nếu không có giỏ hàng hoạt động
            return NotFound($"Active cart for user {username} not found.");
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
    // Thêm NHIỀU sản phẩm vào giỏ hàng
    // ************************************************************
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddItemsToCartDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || !request.Items.Any() || request.Items.Any(i => i.Quantity <= 0))
        {
            return BadRequest("Invalid request: Username is required and at least one item with positive quantity must be provided.");
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return NotFound($"User '{request.Username}' not found.");

        // 💡 Tìm hoặc tạo Cart MỚI (chưa xử lý)
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

    // ************************************************************
    // 3. Endpoint: POST /api/Cart/remove
    // Xóa/Giảm số lượng sản phẩm khỏi giỏ hàng
    // ************************************************************
    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromCart(string username, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return BadRequest("Quantity must be positive for removal.");
        }

        // Tìm Cart CHƯA XỬ LÝ
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
            // Xóa hoàn toàn mục sản phẩm
            _context.CartItems.Remove(cartItem);
            message = $"Product ID {productId} has been completely removed from the cart.";
        }
        else
        {
            // Chỉ giảm số lượng
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

    // ************************************************************
    // 4. Endpoint: POST /api/Cart/checkout
    // Chuyển giỏ hàng thành Đơn hàng (Lưu lịch sử)
    // ************************************************************
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(string username)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            // Tìm giỏ hàng CHƯA XỬ LÝ để thanh toán
            .FirstOrDefaultAsync(c => c.Username == username && c.IsProcessed == false);

        if (cart == null || !cart.CartItems.Any())
        {
            return BadRequest("Active cart is empty or not found.");
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

        // 3. 🔥 ĐÁNH DẤU GIỎ HÀNG ĐÃ XỬ LÝ
        cart.IsProcessed = true;

        // Không cần xóa CartItems vì chúng ta giữ lại Cart (với IsProcessed = true)
        // và sẽ tạo một Cart mới khi user add product lần tiếp theo.

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
    // 5. Endpoint: GET /api/Cart/history/{username}
    // Lấy lịch sử đơn hàng của người dùng
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
    [HttpGet("Restaurant/Processing")]
    public async Task<ActionResult<IEnumerable<OrderHistoryDto>>> GetProcessingOrders()
    {
        // 💡 LẤY CÁC ĐƠN HÀNG CÓ STATUS LÀ "Processed"
        var orders = await _context.Orders
            .Where(o => o.Status == "Processed") // Hoặc dùng OrderStatus.Processed nếu bạn dùng Enum
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderBy(o => o.OrderDate) // Sắp xếp theo thứ tự cũ nhất làm trước
            .ToListAsync();

        if (!orders.Any())
        {
            return Ok("No orders are currently in the 'Processed' status. ✨");
        }

        var processingOrders = orders.Select(o => new OrderHistoryDto
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

        return Ok(processingOrders);
    }
    [HttpGet("Restaurant/DoneHistory")]
    public async Task<ActionResult<IEnumerable<OrderHistoryDto>>> GetDoneOrderHistory()
    {
        // 💡 LẤY CÁC ĐƠN HÀNG CÓ STATUS LÀ "Done"
        var history = await _context.Orders
            .Where(o => o.Status == "Done")
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate) // Sắp xếp từ mới nhất đến cũ nhất
            .ToListAsync();

        if (!history.Any())
        {
            return Ok(new { Message = "Chưa có đơn hàng nào được đánh dấu 'Done' trong hệ thống. 🎉" });
        }

        var historyViewModels = history.Select(o => new OrderHistoryDto
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
    [HttpPost("Restaurant/CheckDone/{orderId}")]
    public async Task<IActionResult> CheckOrderDone(int orderId)
    {
        // 1. Tìm đơn hàng
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            return NotFound($"Order ID {orderId} not found.");
        }

        // 2. Kiểm tra trạng thái hiện tại
        if (order.Status == "Done")
        {
            return BadRequest($"Order ID {orderId} is already marked as Done.");
        }

        if (order.Status != "Processed")
        {
            // Tránh chuyển các đơn hàng Pending, Cancelled trực tiếp thành Done
            return BadRequest($"Cannot mark order as Done. Current status is '{order.Status}'. Only 'Processed' orders can be marked Done.");
        }

        // 3. Cập nhật trạng thái
        order.Status = "Done"; // Chuyển "Processed" thành "Done"

        await _context.SaveChangesAsync();

        return Ok(new
        {
            OrderId = orderId,
            Message = "Order status updated successfully.",
            NewStatus = order.Status
        });
    }
    // ************************************************************
    // 6. Endpoint: GET /api/Cart/AllCarts
    // Lấy tất cả giỏ hàng (bao gồm cả đã xử lý)
    // ************************************************************
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

        // Trong phương thức GetAllCarts
        var allCartsViewModel = allCarts.Select(cart => new AllCartsDto
        {
            CartId = cart.Id,
            Username = cart.Username,
            TotalItems = cart.CartItems.Count,
            TotalQuantity = cart.CartItems.Sum(ci => ci.Quantity),

            // 🔥 SỬ DỤNG THUỘC TÍNH MỚI
            IsProcessed = cart.IsProcessed,

            // Ánh xạ danh sách Items
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