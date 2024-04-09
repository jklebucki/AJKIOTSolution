using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Services
{
    public interface IApiService
    {
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string username);
        Task<ApiResponse<IotDevice>> CreateUserDeviceAsync(IotDevice device);

    }
}
