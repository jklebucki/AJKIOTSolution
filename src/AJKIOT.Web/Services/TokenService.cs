using AJKIOT.Web.Data;

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
    }
}
