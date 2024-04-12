using AJKIOT.Shared.Models;
using AJKIOT.Web.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AJKIOT.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly LocalStorageService _localStorageService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ITokenService tokenService, LocalStorageService localStorageService, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _localStorageService = localStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(AuthRequest authRequest)
        {
            try
            {
                var authResponse = await _httpClient.PostAsJsonAsync("api/Users/login", authRequest);
                var loginResponse = await authResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                if (!authResponse.IsSuccessStatusCode)
                    return loginResponse!;

                if (loginResponse != null && loginResponse.Data != null)
                    await _tokenService.SaveToken(new ApplicationUser
                    {
                        Username = loginResponse.Data.Username!,
                        Email = loginResponse.Data.Email!,
                        Credentials = new UserCredentials()
                        {
                            AccessToken = loginResponse.Data.Tokens![0],
                            RefreshToken = loginResponse.Data.Tokens[1]
                        }
                    });
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.Data!.Tokens![0]);
                return loginResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponse>() { Data = null, Errors = new List<string>() { ex.Message } };
            }



        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegistrationRequest registrationRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/register", registrationRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!;
                }

                var registerResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                if (registerResponse == null) return (await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!;
                await _tokenService.SaveToken(new ApplicationUser
                {
                    Username = registerResponse.Data!.Username!,
                    Email = registerResponse.Data.Email!,
                    Credentials = new UserCredentials() { AccessToken = registerResponse.Data.Tokens![0], RefreshToken = registerResponse.Data.Tokens[1] }
                });
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResponse.Data.Tokens[0]);
                return registerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ApiResponse<AuthResponse>() { Data = null, Errors = new List<string>() { ex.Message } };
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordRequestAsync(ResetPasswordCustomRequest resetPasswordCustomRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/reset-password-request", resetPasswordCustomRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!;
                }

                var resetPasswordResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (resetPasswordResponse == null) return (await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!;

                return resetPasswordResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ApiResponse<bool>() { Data = false, Errors = new List<string>() { ex.Message } };
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordConfirmRequest resetPasswordRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/reset-password-confirm", resetPasswordRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!;
                }

                var resetPasswordResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (resetPasswordResponse == null) return (await response.Content.ReadFromJsonAsync<ApiResponse<bool>>())!;

                return resetPasswordResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ApiResponse<bool>() { Data = false, Errors = new List<string>() { ex.Message } };
            }
        }

        public async Task LogoutAsync()
        {
            await _tokenService.ClearToken();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var applicationUser = await _localStorageService.GetApplicationUserAsync();
            if (applicationUser == null) return false;
            if (applicationUser == null || string.IsNullOrEmpty(applicationUser.Credentials?.RefreshToken) || string.IsNullOrEmpty(applicationUser.Credentials?.AccessToken))
            {
                return false;
            }

            var refreshResponse = await _httpClient.PostAsJsonAsync("api/Users/refresh", new { applicationUser.Credentials.RefreshToken });

            if (!refreshResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
            if (loginResponse == null || loginResponse.Tokens == null)
                return false;
            var refreshedUser = new ApplicationUser { Username = loginResponse.Username!, Email = loginResponse.Email!, Credentials = new UserCredentials { AccessToken = loginResponse.Tokens[0], RefreshToken = loginResponse.Tokens[1] } };
            await _localStorageService.SaveApplicationUserAsync(refreshedUser);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Tokens[0]);
            return true;
        }

    }
}
