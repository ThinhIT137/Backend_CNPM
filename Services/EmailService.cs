using backend.Models;
using backend.Services;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(User user, string emailSubject, string emailBody)
        {
            var senderEmail = _config["EmailSettings:Email"];
            var senderPassword = _config["EmailSettings:Password"];

            Console.WriteLine($"Đang test -> Email: '{senderEmail}' | Pass: '{senderPassword}'");

            var emailMessage = new MimeKit.MimeMessage();
            emailMessage.From.Add(new MimeKit.MailboxAddress("App Du Lịch", senderEmail));
            emailMessage.To.Add(new MimeKit.MailboxAddress(user.Name, user.Email));
            emailMessage.Subject = emailSubject;

            var bodyBuilder = new MimeKit.BodyBuilder { HtmlBody = emailBody };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
