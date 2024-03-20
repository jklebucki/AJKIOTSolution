using System.Net.Http.Json;

namespace AJKIOT.Web.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public EmailSenderService(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailData = new
            {
                From = "AJKIOT",
                To = to,
                Subject = subject,
                Body = body
            };
            await _tokenService.AddTokenToHeader(_httpClient);
            var response = await _httpClient.PostAsJsonAsync("api/Email/send", emailData);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Nie udało się wysłać e-maila.");
            }
        }
    }

}
