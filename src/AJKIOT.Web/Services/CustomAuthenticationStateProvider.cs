using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace AJKIOT.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authenticationService;
        private readonly LocalStorageService _localStorageService;

        public CustomAuthenticationStateProvider(IAuthService authenticationService, LocalStorageService localStorageService)
        {
            _authenticationService = authenticationService;
            _localStorageService = localStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            var currentUser = await _localStorageService.GetApplicationUserAsync();
            if (currentUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, currentUser.Username),
                    new Claim(ClaimTypes.Role, "User")
                };
                claims.AddRange(ParseClaimsFromJwt(currentUser.Credentials!.AccessToken!));
                identity = new ClaimsIdentity(claims, "Bearer");
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
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            keyValuePairs.TryGetValue(ClaimTypes.Name, out var name);
            claims.Add(new Claim(ClaimTypes.Name, name.ToString()));
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
