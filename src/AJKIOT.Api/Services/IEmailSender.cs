using MimeKit;

namespace AJKIOT.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(MailboxAddress from, MailboxAddress to, string subject, string textBody, string htmlBody);
        Task SendResetPasswordEmailAsync(string email, string? userName, string resetLink);
        Task SendResetPasswordConfirmationEmailAsync(string? email, string? userName, string appLink);
        Task SendWelcomeEmailAsync(string username, string email, string appLink);
    }
}
