using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
        Task<bool> RegisterAsync(RegistrationRequest registrationRequest);
    }
}
