using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IDeviceData
    {
        Task<IotDevice> GetDeviceAsync(int deviceId);
    }
}
