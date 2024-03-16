using AJKIOT.Shared.Models;

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

        public async Task SaveToken(UserCredentials tokenResponse)
        {
            await _localStorageService.SaveTokenAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken);
        }

        public async Task<UserCredentials> GetSavedToken()
        {
            return await _localStorageService.GetTokenAsync();
        }

        public async Task ClearToken()
        {
            await _localStorageService.ClearTokenAsync();
        }
    }
}
