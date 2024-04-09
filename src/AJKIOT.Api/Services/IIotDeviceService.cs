using AJKIOT.Api.Data.Migrations;
using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IIotDeviceService
    {
        Task<ApiResponse<IotDevice>> AddDeviceAsync(IotDevice iotDevice);
        Task<ApiResponse<IotDevice>> DeleteDeviceAsync(IotDevice iotDevice);
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesDevicesAsync(string userId);
        Task<ApiResponse<IotDevice>> UpdateDeviceAsync(IotDevice iotDevice);
    }
}
