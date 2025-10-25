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
        private int GetCurrentUserId()
        {
            // Trong thực tế, bạn sẽ lấy từ Claims/Token (ví dụ: return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));)
            return 1;
        }

        // ====================================================================
        // ENDPOINT 1: TẠO URL TẠO TOKEN
        // ====================================================================
        [HttpGet("CreateTokenUrl")]
        public IActionResult CreateTokenUrl()
        {
            int currentUserId = GetCurrentUserId();
            string tokenUrl = _vnPayService.CreateTokenizationUrl(currentUserId, HttpContext);
            return Ok(new { TokenUrl = tokenUrl });
        }

        // ====================================================================
        // ENDPOINT 2: TẠO URL THANH TOÁN BẰNG TOKEN
        // ====================================================================
        [HttpGet("PayWithToken")]
        public async Task<IActionResult> PayWithToken([FromQuery] int orderId, [FromQuery] string token)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return BadRequest("Không tìm thấy Order!");
            if (order.Status != "Processed") return BadRequest("Order Đã được trả tiền rồi");

            string paymentUrl = _vnPayService.CreatePaymentTokenUrl(
                order.Id,
                order.TotalAmount,
                token,
                HttpContext
            );
            return Ok(new { PaymentUrl = paymentUrl });
        }

        // ====================================================================
        // ENDPOINT 3: TẠO URL XÓA TOKEN
        // ====================================================================
        [HttpGet("RemoveToken")]
        public IActionResult RemoveToken([FromQuery] string token)
        {
            int currentUserId = GetCurrentUserId();
            string removeUrl = _vnPayService.CreateRemoveTokenUrl(token, currentUserId, HttpContext);

            // Xóa khỏi DB cục bộ trước (có thể chuyển sang callback)
            var localToken = _context.VnPayCardTokens.FirstOrDefault(t => t.Token == token && t.UserId == currentUserId);
            if (localToken != null) _context.VnPayCardTokens.Remove(localToken);
            _context.SaveChangesAsync();

            return Ok(new { RemoveUrl = removeUrl });
        }


        // ====================================================================
        // ENDPOINT CALLBACK: XỬ LÝ KẾT QUẢ TẠO TOKEN
        // Route: /api/VnPay/TokenCreationReturn
        // ====================================================================
        [HttpGet("TokenCreationReturn")]
        public async Task<IActionResult> TokenCreationReturn()
        {
            var collections = Request.Query;
            if (!_vnPayService.ValidateVnPayHash(collections)) return BadRequest("Invalid Hash Signature.");

            string responseCode = collections["vnp_response_code"]!;
            string transactionStatus = collections["vnp_transaction_status"]!;
            string cardNumber = collections["vnp_card_number"]!.ToString();
            if (responseCode == "00" && transactionStatus == "00")
            {
                // Logic lưu Token (Giả định Model VnPayCardToken)
                var newToken = new VnPayCardToken
                {
                    UserId = int.Parse(collections["vnp_app_user_id"]!),
                    Token = collections["vnp_token"]!,
                    CardNumber = cardNumber.Replace("x", ""),
                    BankCode = collections["vnp_bank_code"]!,
                };
                _context.VnPayCardTokens.Add(newToken);
                await _context.SaveChangesAsync();

                return Ok("Token created and saved successfully. ✅");
            }
            return BadRequest($"Token creation failed. Response: {responseCode}.");
        }

        // ====================================================================
        // ENDPOINT CALLBACK: XỬ LÝ KẾT QUẢ THANH TOÁN
        // Route: /api/VnPay/PaymentTokenReturn
        // ====================================================================
        [HttpGet("PaymentTokenReturn")]
        public async Task<IActionResult> PaymentTokenReturn()
        {
            var collections = Request.Query;
            if (!_vnPayService.ValidateVnPayHash(collections)) return BadRequest("Invalid Hash Signature. 🚨");

            int orderId = int.Parse(collections["vnp_txn_ref"]!);
            string responseCode = collections["vnp_response_code"]!;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            if (responseCode == "00" && collections["vnp_transaction_status"] == "00")
            {
                // Giao dịch thành công
                if (order.Status != "Paid") // Ngăn chặn xử lý trùng lặp
                {
                    order.Status = "Paid";
                    await _context.SaveChangesAsync();
                }
                return Ok($"Order {orderId} paid successfully. Status: Paid. ✅");
            }
            else
            {
                // Thanh toán thất bại
                order.Status = "PaymentFailed";
                await _context.SaveChangesAsync();
                return BadRequest($"Payment failed for Order {orderId}. Response Code: {responseCode}. ❌");
            }
        }

        // ************************************************************
        // TÙY CHỌN: Xử lý Callback Xóa Token (TokenRemoveReturn)
        // ************************************************************
    }
}