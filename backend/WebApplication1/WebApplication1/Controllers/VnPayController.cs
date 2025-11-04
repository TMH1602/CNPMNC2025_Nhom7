// File: WebApplication1.Controllers/VnPayController.cs
using System.Linq; // BẮT BUỘC cho FirstOrDefault, Where, Select, Any, v.v.
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims; // Cần thiết để lấy User ID

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IVnPayService2 _vnPayService2;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public VnPayController(IVnPayService vnPayService, IVnPayService2 vnPayService2, ApplicationDbContext context, IEmailService emailService)
        {
            _vnPayService = vnPayService;
            _vnPayService2 = vnPayService2;
            _context = context;
            _emailService = emailService;
        }

        // 💡 Giả định hàm lấy ID người dùng hiện tại
        [HttpGet("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromQuery] int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status == "Paid") return BadRequest("Order không hợp lệ hoặc đã được thanh toán.");

            // Tạo thông tin OrderInfo
            string orderInfo = $"Thanhtoandonhang{order.Id}";

            string paymentUrl = _vnPayService.CreatePaymentUrl(
                order.Id,
                order.TotalAmount,
                orderInfo,
                HttpContext
            );

            return Ok(new { PaymentUrl = paymentUrl });
        }
        [HttpGet("CreatePayment2")]
        public async Task<IActionResult> CreatePayment2([FromQuery] int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status == "Paid") return BadRequest("Order không hợp lệ hoặc đã được thanh toán.");

            // Tạo thông tin OrderInfo
            string orderInfo = $"Thanhtoandonhang{order.Id}";

            string paymentUrl = _vnPayService2.CreatePaymentUrl2(
                order.Id,
                order.TotalAmount,
                orderInfo,
                HttpContext
            );

            return Ok(new { PaymentUrl = paymentUrl });
        }
        [HttpGet("VnpayReturn")]
        public async Task<IActionResult> VnpayReturn()
        {
            var collections = Request.Query;

            // 1. Kiểm tra Hash
            if (!_vnPayService.ValidateVnPayHash(collections))
            {
                return BadRequest("Invalid Hash Signature. 🚨");
            }

            // 2. Lấy thông tin giao dịch
            int orderId = int.Parse(collections["vnp_TxnRef"]!);
            string responseCode = collections["vnp_ResponseCode"]!;
            string transactionStatus = collections["vnp_TransactionStatus"]!;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            // 3. Xử lý trạng thái
            if (responseCode == "00" && transactionStatus == "00")
            {
                // Giao dịch thành công
                if (order.Status != "Paid")
                {
                    order.Status = "Paid";
                    
                    await _context.SaveChangesAsync();

                    await SendConfirmationEmail(order);
                }
                return Redirect("https://localhost:5000/Checkout/Success");
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                order.Status = "Processed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }
        [HttpGet("VnpayReturn2")]
        public async Task<IActionResult> VnpayReturn2()
        {
            var collections = Request.Query;

            // 1. Kiểm tra Hash
            if (!_vnPayService.ValidateVnPayHash(collections))
            {
                return BadRequest("Invalid Hash Signature. 🚨");
            }

            // 2. Lấy thông tin giao dịch
            int orderId = int.Parse(collections["vnp_TxnRef"]!);
            string responseCode = collections["vnp_ResponseCode"]!;
            string transactionStatus = collections["vnp_TransactionStatus"]!;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            // 3. Xử lý trạng thái
            if (responseCode == "00" && transactionStatus == "00")
            {
                // Giao dịch thành công
                if (order.Status != "Paid")
                {
                    order.Status = "Paid";
                    await _context.SaveChangesAsync();
                    await SendConfirmationEmail(order);
                }
                return Redirect("https://10.0.2.2:5000/Checkout/Success");
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                order.Status = "Processed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }
        private async Task SendConfirmationEmail(Order order)
        {
            // 1. Tìm thông tin người dùng để lấy Email
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == order.Username);

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string emailSubject = $"🎉 Xác nhận Đơn hàng #{order.Id} đã thanh toán thành công!";
                string emailBody = CreateOrderConfirmationEmailBody(order);

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }
        }

        private string CreateOrderConfirmationEmailBody(Order order)
        {
            var itemDetails = string.Join("", order.OrderDetails.Select(od =>
                $@"<tr>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{od.Product?.Name ?? "Sản phẩm đã xóa"}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{od.Quantity}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{od.PriceAtTime:N0} VND</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{(od.Quantity * od.PriceAtTime):N0} VND</td>
                  </tr>"
            ));

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <div style='max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                        <h1 style='color: #4CAF50;'>Cảm ơn bạn đã mua hàng!</h1>
                        <p>Đơn hàng <strong>#{order.Id}</strong> của bạn đã được thanh toán thành công vào lúc {order.OrderDate.ToLocalTime():HH:mm:ss dd/MM/yyyy}.</p>
                        
                        <h2>Cảm ơn Người dùng có tài khoản#{order.Username}đã thanh toán và tin tưởng chúng tôi</h2>
                        <p>Chúng tôi rất cảm kích #{order.Username} đã sử dụng sản phẩm của chúng tôi chúc bạn 1 ngày tốt lành</p>
                        <p>Vui lòng kiểm tra điện thoại và đơn hàng của bạn đã được cập nhật trên web <3 </p>
                        
                        <h3 style='color: #333;'>Tổng cộng: <strong>{order.TotalAmount:N0} VND</strong></h3>
                        <p>Chúng tôi sẽ xử lý đơn hàng của bạn sớm nhất.</p>
                        <p>Trân trọng,<br>Đội ngũ CÔNG NGHỆ PHẦN MỀM HUFLIT</p>
                    </div>
                </body>
                </html>";
        }
    }

}