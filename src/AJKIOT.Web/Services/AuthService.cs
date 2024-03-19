using AJKIOT.Shared.Models;
using AJKIOT.Web.Data;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AJKIOT.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly NavigationManager _navigationManager;
        private readonly LocalStorageService _localStorageService;

        public AuthService(HttpClient httpClient, ITokenService tokenService, NavigationManager navigationManager, LocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = new HttpResponseMessage();
            try
            {
                response = await _httpClient.PostAsJsonAsync("api/Users/login", new AuthRequest { Email = email, Password = password });
            }
            catch (Exception ex)
            {
                return false;
            }


            if (!response.IsSuccessStatusCode) return false;

            var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (loginResponse == null) return false;
            await _tokenService.SaveToken(new ApplicationUser { Username = loginResponse.Username!, Email = loginResponse.Email!, Credentials = new UserCredentials() { AccessToken = loginResponse.Token[0], RefreshToken = loginResponse.Token[1] } });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token[0]);
            return true;
        }

        public async Task<bool> RegisterAsync(RegistrationRequest registrationRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", registrationRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var registerResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (registerResponse == null) return false;
            await _tokenService.SaveToken(new ApplicationUser { Username = registerResponse.Username!, Email = registerResponse.Email!, Credentials = new UserCredentials() { AccessToken = registerResponse.Token[0], RefreshToken = registerResponse.Token[1] } });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResponse.Token[0]);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _tokenService.ClearToken();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _navigationManager.NavigateTo("/login");
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var applicationUser = await _localStorageService.GetApplicationUserAsync();
            if (applicationUser == null) return false;
            if (applicationUser == null || string.IsNullOrEmpty(applicationUser.Credentials?.RefreshToken) || string.IsNullOrEmpty(applicationUser.Credentials?.AccessToken))
            {
                return false;
            }

            var refreshResponse = await _httpClient.PostAsJsonAsync("api/Users/refresh", new { RefreshToken = applicationUser.Credentials.RefreshToken });

            if (!refreshResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
            if (loginResponse == null || loginResponse.Token == null)
                return false;
            var newUser = new ApplicationUser { Username = loginResponse.Username!, Email = loginResponse.Email!, Credentials = new UserCredentials { AccessToken = loginResponse.Token[0], RefreshToken = loginResponse.Token[1] } };
            await _localStorageService.SaveApplicationUserAsync(newUser);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token[0]);
            return true;
        }
    }
}
