using AJKIOT.Api.Models;
using MailKit.Net.Smtp;
using MimeKit;


namespace AJKIOT.Api.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly ITemplateService _templateService;
        public EmailSenderService(SmtpSettings smtpSettings, ILogger<EmailSenderService> logger, ITemplateService templateService)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
            _templateService = templateService;
        }
        public async Task SendEmailAsync(MailboxAddress from, MailboxAddress to, string subject, string body, bool isHtml)
        {
            var message = new MimeMessage();
            message.From.Add(from);
            message.To.Add(to);
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = body;
            message.Body = builder.ToMessageBody();

            try
            {
                await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
            }

        }

        public async Task SendResetPasswordConfirmationEmailAsync(string? email, string? userName, string appLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AJKIOT System", "ajkiot@ajksoftware.pl"));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = "Your AJKIOT Password Has Been Reset";

            var builder = new BodyBuilder();
            var body = await _templateService.GetTemplateAsync("ResetPasswordConfirmationEmail.html");
            if (body != string.Empty)
            {
                body = body.Replace("[link]", appLink).Replace("[username]", userName);
                builder.HtmlBody = body;
                message.Body = builder.ToMessageBody();

                try
                {
                    await SendMessageAsync(message);
                    _logger.LogInformation($"Password reset confirmation email sent to: {email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send password reset confirmation email to {email}: {ex.Message}");
                }
            }
            else
            {
                _logger.LogError("Failed to load the ResetPasswordConfirmationEmail.html template.");
            }
        }

        public async Task SendResetPasswordEmailAsync(string email, string? userName, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AJKIOT System", "ajkiot@ajksoftware.pl"));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = "AJKIOT - Password Reset";
            var builder = new BodyBuilder();
            var body = await _templateService.GetTemplateAsync("ResetPasswordEmail.html");
            if (body != string.Empty)
            {
                body = body.Replace("[link]", resetLink).Replace("[username]", userName);
                message.Body = builder.ToMessageBody();
                try
                {
                    await SendMessageAsync(message);
                    _logger.LogInformation($"Reset password email sent to: {email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
            }
        }

        public async Task SendWelcomeEmailAsync(string username, string email, string appLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AJKIOT System", "ajkiot@ajksoftware.pl"));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "Welcome to AJKIOT";

            var builder = new BodyBuilder();
            var body = await _templateService.GetTemplateAsync("WelcomeEmail.html");
            if (body != string.Empty)
            {
                builder.HtmlBody = body.Replace("[link]", appLink).Replace("[username]", username);
                message.Body = builder.ToMessageBody();

                try
                {
                    await SendMessageAsync(message);
                    _logger.LogInformation($"Welcome email sent to: {email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
            }
        }

        private async Task SendMessageAsync(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
