using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IEmailSender _emailSender;
        public EmailController(ILogger<EmailController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send(Email email)
        {
            var from = new MailboxAddress(email.FromName, email.FromEmail);
            var to = new MailboxAddress(email.ToName, email.ToEmail);
            await _emailSender.SendEmailAsync(from, to, email.Subject, email.Body, email.IsHtml);
            _logger.LogInformation($"Email from {email.FromEmail} to {email.ToEmail} sent");
            return Ok();
        }
    }
}
