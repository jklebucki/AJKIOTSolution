using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<AuthResponse>> AuthenticateUserAsync(AuthRequest request);
        Task<ServiceResponse<RegistrationRequest>> RegisterUserAsync(RegistrationRequest request);
    }
}
