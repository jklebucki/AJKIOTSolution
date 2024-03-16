using AJKIOT.Api.Models;

namespace AJKIOT.Api.Services
{
    public interface ITokenService
    {
        string[] CreateToken(ApplicationUser user);
    }
}
