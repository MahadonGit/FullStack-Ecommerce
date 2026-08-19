using Ecommerce.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Ecommerce.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var senderName =
                _configuration["EmailSettings:SenderName"];

            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var password =
                _configuration["EmailSettings:Password"];

            var host =
                _configuration["EmailSettings:Host"];

            var port =
                _configuration["EmailSettings:Port"];

            // Validate configuration
            if (string.IsNullOrWhiteSpace(senderName))
                throw new InvalidOperationException(
                    "EmailSettings:SenderName is missing.");

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException(
                    "EmailSettings:SenderEmail is missing.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "EmailSettings:Password is missing.");

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException(
                    "EmailSettings:Host is missing.");

            if (string.IsNullOrWhiteSpace(port))
                throw new InvalidOperationException(
                    "EmailSettings:Port is missing.");

            if (!int.TryParse(port, out var portNumber))
                throw new InvalidOperationException(
                    "EmailSettings:Port must be a valid number.");

            // Create email
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    senderName,
                    senderEmail));

            message.To.Add(
                MailboxAddress.Parse(toEmail));

            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = body
            }.ToMessageBody();

            // Connect to SMTP server
            using var client = new SmtpClient();

            await client.ConnectAsync(
                host,
                portNumber,
                SecureSocketOptions.StartTls);

            // Authenticate
            await client.AuthenticateAsync(
                senderEmail,
                password);

            // Send
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);
        }
    }
}