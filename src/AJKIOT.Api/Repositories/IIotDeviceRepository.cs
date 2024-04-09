using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Repositories
{
    public interface IIotDeviceRepository
    {
        Task<int> AddDeviceAsync(IotDevice device);
        Task<IEnumerable<IotDevice>> GetUserDevicesAsync(string userId);
        Task<IotDevice> GetDeviceAsync(string userId, int deviceId);
        Task<IotDevice> UpdateDeviceAsync(IotDevice device);

    }
}
