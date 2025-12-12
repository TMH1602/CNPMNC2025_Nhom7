using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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

        [HttpGet("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromQuery] int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status == "Paid") return BadRequest("Order không hợp lệ hoặc đã được thanh toán.");
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
            if (!_vnPayService.ValidateVnPayHash(collections))
            {
                return BadRequest("Invalid Hash Signature. 🚨");
            }
            int orderId = int.Parse(collections["vnp_TxnRef"]!);
            string responseCode = collections["vnp_ResponseCode"]!;
            string transactionStatus = collections["vnp_TransactionStatus"]!;
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound("Order not found.");
            if (responseCode == "00" && transactionStatus == "00")
            {
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
                order.Status = "Processed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }

        [HttpGet("VnpayReturn2")]
        public async Task<IActionResult> VnpayReturn2()
        {
            var collections = Request.Query;
            if (!_vnPayService.ValidateVnPayHash(collections))
            {
                return BadRequest("Invalid Hash Signature. 🚨");
            }
            int orderId = int.Parse(collections["vnp_TxnRef"]!);
            string responseCode = collections["vnp_ResponseCode"]!;
            string transactionStatus = collections["vnp_TransactionStatus"]!;
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound("Order not found.");
            if (responseCode == "00" && transactionStatus == "00")
            {
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
                order.Status = "Processed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }

        private async Task SendConfirmationEmail(Order order)
        {
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
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>
                        {od.Product?.Id ?? 0}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>
                        {od.Product?.Name ?? "Sản phẩm không tồn tại"}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>
                        {od.Quantity}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>
                        {od.PriceAtTime:N0} VND
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>
                        {(od.Quantity * od.PriceAtTime):N0} VND
                    </td>
                </tr>"
            ));

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 8px;'>
                        <h1 style='color: #4CAF50; text-align: center;'>Thanh Toán Thành Công!</h1>
                        
                        <p>Xin chào <strong>{order.Username}</strong>,</p>
                        <p>Đơn hàng <strong>#{order.Id}</strong> của bạn đã được thanh toán thành công vào lúc {order.OrderDate.ToLocalTime():HH:mm:ss dd/MM/yyyy}.</p>
                        
                        <h3>Chi tiết đơn hàng:</h3>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>
                            <thead>
                                <tr style='background-color: #f2f2f2;'>
                                    <th style='border: 1px solid #ddd; padding: 8px;'>Mã SP</th>
                                    <th style='border: 1px solid #ddd; padding: 8px;'>Tên Sản phẩm</th>
                                    <th style='border: 1px solid #ddd; padding: 8px;'>SL</th>
                                    <th style='border: 1px solid #ddd; padding: 8px;'>Đơn giá</th>
                                    <th style='border: 1px solid #ddd; padding: 8px;'>Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemDetails}
                            </tbody>
                        </table>

                        <h3 style='text-align: right; color: #d32f2f; margin-top: 15px;'>
                            Tổng thanh toán: {order.TotalAmount:N0} VND
                        </h3>

                        <p>Cảm ơn bạn đã tin tưởng và mua sắm tại CÔNG NGHỆ PHẦN MỀM HUFLIT.</p>
                        <p>Trân trọng,<br>Đội ngũ hỗ trợ.</p>
                    </div>
                </body>
                </html>";
        }
    }
}