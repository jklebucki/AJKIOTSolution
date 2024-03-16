using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface ITokenService
    {
        Task SaveToken(UserCredentials tokens);
        Task<UserCredentials> GetSavedToken();
        Task ClearToken();
    }
}
