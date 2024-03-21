using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace AJKIOT.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly LocalStorageService _localStorageService;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;

        public CustomAuthenticationStateProvider(IAuthService authenticationService, LocalStorageService localStorageService, ILogger<CustomAuthenticationStateProvider> logger)
        {
            _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var currentUser = await _localStorageService.GetApplicationUserAsync();
                if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Credentials?.AccessToken))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, currentUser.Username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                    claims.AddRange(ParseClaimsFromJwt(currentUser.Credentials.AccessToken));
                    identity = new ClaimsIdentity(claims, "Bearer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication()
        {
            var authState = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
                if (keyValuePairs != null)
                {
                    foreach (var keyValuePair in keyValuePairs)
                    {
                        claims.Add(new Claim(keyValuePair.Key, keyValuePair.Value.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
