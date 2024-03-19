using AJKIOT.Web.Data;

namespace AJKIOT.Web.Services
{
    public interface ITokenService
    {
        Task SaveToken(ApplicationUser applicationUser);
        Task<ApplicationUser> GetSavedToken();
        Task ClearToken();
    }
}
