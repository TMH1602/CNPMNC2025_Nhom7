using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using System.Net.Sockets; // Cần thiết cho AddressFamily
using System.Net; // Cần thiết cho IPAddress
using Microsoft.Extensions.Logging;
using System.Web; // <-- THÊM DÒNG NÀY
namespace WebApplication1.Services
{
    // Giả định IVnPayService đã được định nghĩa ở đâu đó


    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<VnPayService> _logger; // <-- KHAI BÁO LOGGER
        public VnPayService(IConfiguration config, ILogger<VnPayService> logger)
        {
            _config = config;
            _logger = logger; // <-- GÁN LOGGER
        }
        private string HmacSha512(string key, string inputData)
        {
            // Initialize StringBuilder to build the final hash string
            var hash = new StringBuilder();

            // Convert the Secret Key to a byte array using UTF8 encoding
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
        private string GetValidIpAddress(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp == null) return "127.0.0.1";

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            // Trường hợp IPv6 Localhost (::1)
            else if (remoteIp.AddressFamily == AddressFamily.InterNetworkV6 && remoteIp.ToString() == "::1")
            {
                return "127.0.0.1";
            }
            // Trường hợp IPv4 bình thường
            else if (remoteIp.AddressFamily == AddressFamily.InterNetwork)
            {
                return remoteIp.ToString();
            }

            return "127.0.0.1"; // Giá trị mặc định an toàn
        }
        public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, HttpContext context)
        {
            // 1. Lấy cấu hình
            string tmnCode = _config["Vnpay:TmnCode"] ?? throw new ArgumentNullException("TmnCode is missing.");
            string hashSecret = _config["Vnpay:HashSecret"] ?? throw new ArgumentNullException("HashSecret is missing.");
            string baseUrl = _config["Vnpay:PaymentUrl"] ?? throw new ArgumentNullException("PaymentUrl is missing.");
            string returnUrl = _config["Vnpay:ReturnUrl"] ?? "";

            // Đảm bảo không có dấu và xử lý lỗi encoding trước khi hash
            string encodedOrderInfo = HttpUtility.UrlEncode(orderInfo, Encoding.GetEncoding("iso-8859-1"));

            string expireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");

            // 2. Chuẩn bị tham số (SortedList tự động sắp xếp A-Z cho Hash)
            var vnpParams = new SortedList<string, string>
            {
                {"vnp_Amount", ((long)amount * 100).ToString()},
                {"vnp_Command", "pay"},
                {"vnp_CreateDate",DateTime.Now.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", "VND"},
                {"vnp_IpAddr", GetValidIpAddress(context)},
                {"vnp_Locale", "vn"},
                {"vnp_OrderInfo", encodedOrderInfo},
                {"vnp_OrderType", "180000"},
                {"vnp_ReturnUrl", returnUrl},
                {"vnp_TmnCode", tmnCode},
                {"vnp_TxnRef", orderId.ToString()},
                {"vnp_Version", "2.0.0"},
                {"vnp_ExpireDate", expireDate}, 
            };

            // 3. Tạo chuỗi Hash và URL
            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            string secureHash = HmacSha512(hashSecret, dataHash);

            // 4. Hoàn thiện URL (Sử dụng vnp_SecureHash - S và H hoa)
            return $"{baseUrl}?{dataHash}&vnp_SecureHash={secureHash}";
        }
        public bool ValidateVnPayHash(IQueryCollection collections)
        {
            string hashSecret = _config["Vnpay:HashSecret"] ?? "";

            // LƯU Ý: VNPay V2.1.0 sử dụng vnp_SecureHash (S hoa)
            string receivedHash = collections["vnp_SecureHash"]!.ToString();

            // 1. Lọc và sắp xếp các tham số (trừ Hash)
            var vnpParams = new SortedList<string, string>();
            foreach (var key in collections.Keys)
            {
                // Chỉ lấy các tham số vnp_... và không lấy vnp_SecureHash
                if (key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    // Lấy giá trị chuỗi (cần thiết cho Hash)
                    vnpParams.Add(key, collections[key]!.ToString());
                }
            }

            // 2. Tạo chuỗi Hash Data

            var dataHash = string.Join("&", vnpParams.Select(p => p.Key + "=" + p.Value));
            _logger.LogError("Debug DataHash: {Data}", dataHash);
            // 3. Tính toán lại Hash
            string computedHash = HmacSha512(hashSecret, dataHash);

            // 4. So sánh (Không phân biệt hoa thường)
            return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}