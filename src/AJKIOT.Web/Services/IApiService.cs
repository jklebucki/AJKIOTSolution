using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IApiService
    {
        Task<AuthResponse> LoginAsync(AuthRequest authRequest);

    }
}
