using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(AuthRequest authRequest);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegistrationRequest registrationRequest);
    }
}
