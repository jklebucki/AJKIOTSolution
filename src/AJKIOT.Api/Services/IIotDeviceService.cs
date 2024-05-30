using AJKIOT.Shared.Models;
using AJKIOT.Shared.Settings;

namespace AJKIOT.Api.Services
{
    public interface IIotDeviceService
    {
        Task<ApiResponse<IotDevice>> AddDeviceAsync(IotDevice iotDevice);
        Task<ApiResponse<bool>> DeleteDeviceAsync(int deviceId);
        Task<IotDevice> GetDeviceAsync(int devdeviceId);
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string userId);
        Task<ApiResponse<IotDevice>> UpdateDeviceAsync(IotDevice iotDevice);
        Task<Stream> GetDeviceFirmwareAsStreamAsync(FirmwareSettings firmwareSettings);
        IAsyncEnumerable<string> GetAllowedDevicesAsync();
    }
}
