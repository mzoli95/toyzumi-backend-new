using kz_webshop_be.Repository;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace kz_webshop_be.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is not configured");
                var password = _configuration["Email:Password"] ?? throw new InvalidOperationException("Email:Password is not configured");

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                var mail = new MailMessage(fromEmail, to, subject, body)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                throw new InvalidOperationException("Failed to send email.", ex);
            }
        }
    }

}
