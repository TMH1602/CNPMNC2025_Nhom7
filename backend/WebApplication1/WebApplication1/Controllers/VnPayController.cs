// File: WebApplication1.Controllers/VnPayController.cs
using System.Linq; // BẮT BUỘC cho FirstOrDefault, Where, Select, Any, v.v.
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;

        public VnPayController(IVnPayService vnPayService, ApplicationDbContext context)
        {
            _vnPayService = vnPayService;
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
                return Ok($"Order {orderId} paid successfully. Status: Paid. ✅");
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                order.Status = "PaymentFailed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }
    }
}