using AJKIOT.Web.Data;
using System.Net.Http.Headers;

namespace AJKIOT.Web.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly LocalStorageService _localStorageService;

        public TokenService(ILogger<TokenService> logger, LocalStorageService localStorageService)
        {
            _logger = logger;
            _localStorageService = localStorageService;
        }

        public async Task SaveToken(ApplicationUser applicatioUser)
        {
            await _localStorageService.SaveApplicationUserAsync(applicatioUser);
        }

        public async Task<ApplicationUser> GetSavedToken()
        {
            return await _localStorageService.GetApplicationUserAsync();
        }

        public async Task ClearToken()
        {
            await _localStorageService.ClearTokenAsync();
        }

        public async Task AddTokenToHeader(HttpClient httpClient)
        {
            var token = await GetSavedToken();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Credentials.AccessToken);
        }
    }
}
