using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;

namespace WebApplication1.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        // --- Hàm hỗ trợ Hash ---
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        // --- Hàm hỗ trợ tạo tham số chung ---
        private SortedList<string, string> GetBaseVnpayParams(HttpContext context)
        {
            // Lấy cấu hình
            string tmnCode = _config["Vnpay:TmnCode"] ?? throw new ArgumentNullException("Vnpay:TmnCode");

            return new SortedList<string, string>
            {
                {"vnp_tmn_code", tmnCode},
                {"vnp_version", "2.0.1"},
                {"vnp_locale", "vi"},
                {"vnp_ip_addr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"},
                {"vnp_create_date", DateTime.Now.ToString("yyyyMMddHHmmss")},
            };
        }

        // --- 1. TẠO URL TẠO TOKEN ---
        public string CreateTokenizationUrl(int userId, HttpContext context)
        {
            var vnpParams = GetBaseVnpayParams(context);
            string baseUrl = _config["Vnpay:TokenBaseUrl"] + "create-token.html";
            string hashSecret = _config["Vnpay:HashSecret"] ?? "";

            // Tham số Tokenization cụ thể
            vnpParams.Add("vnp_command", "token_create");
            vnpParams.Add("vnp_app_user_id", userId.ToString());
            vnpParams.Add("vnp_card_type", "01");
            vnpParams.Add("vnp_txn_desc", "Tao moi token");
            vnpParams.Add("vnp_txn_ref", DateTime.Now.Ticks.ToString()); // Transaction ID duy nhất
            vnpParams.Add("vnp_return_url", _config["Vnpay:ReturnTokenCreateUrl"] ?? "");
            vnpParams.Add("vnp_cancel_url", _config["Vnpay:CancelUrl"] ?? "");

            // Tạo chuỗi Hash và URL
            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            string secureHash = HmacSha512(hashSecret, dataHash);
            return $"{baseUrl}?{dataHash}&vnp_secure_hash={secureHash}";
        }

        // --- 2. TẠO URL THANH TOÁN BẰNG TOKEN ---
        public string CreatePaymentTokenUrl(int orderId, decimal amount, string token, HttpContext context)
        {
            var vnpParams = GetBaseVnpayParams(context);
            string baseUrl = _config["Vnpay:TokenBaseUrl"] + "payment-token.html";
            string hashSecret = _config["Vnpay:HashSecret"] ?? "";

            // Tham số Payment Token cụ thể
            vnpParams.Add("vnp_command", "token_pay");
            vnpParams.Add("vnp_amount", ((long)amount * 100).ToString());
            vnpParams.Add("vnp_curr_code", "VND");
            vnpParams.Add("vnp_token", token);
            vnpParams.Add("vnp_app_user_id", "4"); // Lấy User ID thực tế
            vnpParams.Add("vnp_txn_ref", orderId.ToString());
            vnpParams.Add("vnp_txn_desc", $"thanh toan don hang {orderId}");
            vnpParams.Add("vnp_return_url", _config["Vnpay:ReturnPaymentUrl"] ?? "");
            vnpParams.Add("vnp_cancel_url", _config["Vnpay:CancelUrl"] ?? "");

            // Tạo chuỗi Hash và URL
            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            string secureHash = HmacSha512(hashSecret, dataHash);
            return $"{baseUrl}?{dataHash}&vnp_secure_hash={secureHash}";
        }

        // --- 3. TẠO URL XÓA TOKEN ---
        public string CreateRemoveTokenUrl(string token, int userId, HttpContext context)
        {
            var vnpParams = GetBaseVnpayParams(context);
            string baseUrl = _config["Vnpay:TokenBaseUrl"] + "remove-token.html";
            string hashSecret = _config["Vnpay:HashSecret"] ?? "";

            // Tham số Remove Token cụ thể
            vnpParams.Add("vnp_command", "token_remove");
            vnpParams.Add("vnp_token", token);
            vnpParams.Add("vnp_app_user_id", userId.ToString());
            vnpParams.Add("vnp_txn_ref", DateTime.Now.Ticks.ToString()); // ID giao dịch duy nhất
            vnpParams.Add("vnp_txn_desc", "Xoa token da luu");
            vnpParams.Add("vnp_return_url", _config["Vnpay:ReturnTokenRemoveUrl"] ?? "");

            // Tạo chuỗi Hash và URL
            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            string secureHash = HmacSha512(hashSecret, dataHash);
            return $"{baseUrl}?{dataHash}&vnp_secure_hash={secureHash}";
        }

        // --- 4. XÁC MINH HASH (Callback) ---
        public bool ValidateVnPayHash(IQueryCollection collections)
        {
            string hashSecret = _config["Vnpay:HashSecret"] ?? "";
            string receivedHash = collections["vnp_secure_hash"]!;

            // Lấy tất cả tham số trừ vnp_secure_hash
            var vnpParams = new SortedList<string, string>();
            foreach (var key in collections.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_secure_hash")
                {
                    vnpParams.Add(key, collections[key]!);
                }
            }

            // Tạo chuỗi Hash để so sánh
            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            string computedHash = HmacSha512(hashSecret, dataHash);

            return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}