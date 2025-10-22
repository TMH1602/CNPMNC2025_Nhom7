using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RestaurantController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
