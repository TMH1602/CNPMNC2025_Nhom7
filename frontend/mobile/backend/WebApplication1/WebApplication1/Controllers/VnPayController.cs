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

        public VnPayController(IVnPayService vnPayService, IVnPayService2 vnPayService2, ApplicationDbContext context)
        {
            _vnPayService = vnPayService;
            _vnPayService2 = vnPayService2;
            _context = context;
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
                }
                return Redirect("https://10.0.2.2:5000/Checkout/Success");
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                order.Status = "Processed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
                return Redirect("https://10.0.2.2:5000/Checkout/Success");
            }
        }
        }
}