using System.Net.Mail;
using System.Threading.Tasks;
using System;
using WebApplication1.Services;
using Microsoft.Extensions.Configuration; // 💡 Cần thiết để đọc appsettings

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        // 💡 SỬ DỤNG IConfiguration trong Constructor
        public EmailService(IConfiguration configuration)
        {
            // Đọc cấu hình từ section "EmailSettings"
            var emailConfig = configuration.GetSection("EmailSettings");

            _smtpHost = emailConfig["Host"] ?? throw new ArgumentNullException("EmailSettings:Host is missing.");
            _smtpPort = int.Parse(emailConfig["Port"] ?? throw new ArgumentNullException("EmailSettings:Port is missing."));
            _smtpUser = emailConfig["Username"] ?? throw new ArgumentNullException("EmailSettings:Username is missing.");
            _smtpPass = emailConfig["Password"] ?? throw new ArgumentNullException("EmailSettings:Password is missing.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                return;
            }

            // Dùng try-catch để xử lý lỗi gửi mail, không làm gián đoạn luồng chính
            try
            {
                using (var message = new MailMessage())
                using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                {
                    message.From = new MailAddress(_smtpUser, "Tên Cửa Hàng Của Bạn");
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass);

                    await smtpClient.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
                // Bạn có thể cân nhắc gửi cảnh báo nội bộ nếu lỗi này xảy ra thường xuyên
            }
        }
    }
}