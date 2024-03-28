using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(AuthRequest authRequest);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegistrationRequest registrationRequest);
        Task<ApiResponse<bool>> ResetPasswordRequestAsync(ResetPasswordCustomRequest resetPasswordCustomRequest);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordConfirmRequest resetPasswordRequest);
    }
}
