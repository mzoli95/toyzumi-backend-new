namespace kz_webshop_be.Repository
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

}
