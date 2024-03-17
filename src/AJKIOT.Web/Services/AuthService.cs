using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

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
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new AuthRequest { Email = email, Password = password });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            await _tokenService.SaveToken(new UserCredentials() { AccessToken = loginResponse.Token[0], RefreshToken = loginResponse.Token[1] });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token[0]);
            return true;
        }

        public async Task<bool> RegisterAsync(RegistrationRequest registrationRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registrationRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var registerResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            await _tokenService.SaveToken(new UserCredentials() { AccessToken = registerResponse.Token[0], RefreshToken = registerResponse.Token[1] });
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
            var credentials = await _localStorageService.GetTokenAsync();
            if (credentials == null || string.IsNullOrEmpty(credentials?.RefreshToken) || string.IsNullOrEmpty(credentials?.AccessToken))
            {
                return false;
            }

            var refreshResponse = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = credentials.RefreshToken });

            if (!refreshResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
            if (loginResponse == null || loginResponse.Token == null)
                return false;
            await _localStorageService.SaveTokenAsync(loginResponse.Token[0], loginResponse.Token[1]);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token[0]);
            return true;
        }
    }
}
