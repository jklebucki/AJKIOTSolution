using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IApiService
    {
        Task<IEnumerable<IotDevice>> GetDeviceStatusAsync(int userId);

    }
}
