using AJKIOT.Api.Models;
using MailKit.Net.Smtp;
using MimeKit;


namespace AJKIOT.Api.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailSenderService> _logger;
        public EmailSenderService(SmtpSettings smtpSettings, ILogger<EmailSenderService> logger)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
        }
        public async Task SendEmailAsync(MailboxAddress from, MailboxAddress to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(from);
            message.To.Add(to);
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = body;
            message.Body = builder.ToMessageBody();

            await Task.CompletedTask;
            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
            }

        }
    }
}
