using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IUserService
    {
        Task<ApiResponse<AuthResponse>> AuthenticateUserAsync(AuthRequest request);
        Task<ApiResponse<AuthResponse>> RegisterUserAsync(RegistrationRequest request);
        Task<ApiResponse<bool>> SendPasswordResetLinkAsync(ResetPasswordCustomRequest request);
        Task<ApiResponse<bool>> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request);
    }
}
