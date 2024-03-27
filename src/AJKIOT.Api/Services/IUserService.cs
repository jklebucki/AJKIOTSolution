using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity.Data;

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
