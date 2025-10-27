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
                .Where(o => o.Status == "Paid") // Hoặc dùng OrderStatus.Processed nếu bạn dùng Enum
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderBy(o => o.OrderDate) // Sắp xếp theo thứ tự cũ nhất làm trước
                .ToListAsync();

            if (!orders.Any())
            {
                return Ok("Không có đơn hàng nào cần xử lý :))))))✨");
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
                return Ok(new { Message = "Chưa có lịch sử đơn hàng thành công" });
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

            if (order.Status != "Paid")
            {
                // Tránh chuyển các đơn hàng Pending, Cancelled trực tiếp thành Done
                return BadRequest($"Đơn hàng {orderId} chưa được thanh toán cho nên không thể nào có thể done được");
            }

            // 3. Cập nhật trạng thái
            order.Status = "Done"; // Chuyển "Processed" thành "Done"

            await _context.SaveChangesAsync();

            return Ok(new
            {
                OrderId = orderId,
                Message = "Đơn hàng hoàn thành thành công!",
                NewStatus = order.Status
            });
        }
    }
}
