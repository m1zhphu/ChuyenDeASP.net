using MailKit.Net.Smtp;
using MimeKit;
// Đã xóa using System.Net.Mail; để tránh lỗi Ambiguous reference

namespace SmartGarage.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();

            // Cập nhật Email người gửi
            message.From.Add(new MailboxAddress("Smart Garage ERP", "minhphu25102005@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            // Kết nối đến SMTP Gmail
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

            // Xác thực bằng Email và Mật khẩu ứng dụng (đã bỏ dấu cách)
            await client.AuthenticateAsync("minhphu25102005@gmail.com", "hwwlizskwjjlzfge");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}