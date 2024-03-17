using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AJKIOT.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authenticationService;

        public CustomAuthenticationStateProvider(IAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            //if (_authenticationService.CurrentUser != null && !string.IsNullOrEmpty(_authenticationService.CurrentUser.Token))
            //{
            //    // Tutaj możesz dodać więcej informacji na temat użytkownika, jeśli są dostępne
            //    var claims = new[]
            //    {
            //    new Claim(ClaimTypes.Name, _authenticationService.CurrentUser.Username),
            //    // Dodaj inne claims, jeśli są dostępne i potrzebne
            //};
            //    identity = new ClaimsIdentity(claims, "CustomAuth");
            //}

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
    }

}
