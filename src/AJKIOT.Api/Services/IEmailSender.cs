﻿using MimeKit;

namespace AJKIOT.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(MailboxAddress from, MailboxAddress to, string subject, string body, bool isHtml);
        Task SendWelcomeEmailAsync(string username, string email);
    }
}
