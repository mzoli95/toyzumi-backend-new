using kz_webshop_be.Repository;
using System.Net.Mail;
using System.Net;

namespace kz_webshop_be.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    //Sony0714**/
                    Credentials = new NetworkCredential("mzoltan0714@gmail.com", "krvh vrtr imxr bhwr"),
                    EnableSsl = true
                };

                var mail = new MailMessage("mzoltan0714@gmail.com", to, subject, body)
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
