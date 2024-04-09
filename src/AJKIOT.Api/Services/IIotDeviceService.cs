using AJKIOT.Api.Data.Migrations;
using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IIotDeviceService
    {
        Task<ApiResponse<int>> AddDevice(IotDevice iotDevice);
        Task<ApiResponse<bool>> DeleteDevice(IotDevice iotDevice);
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesDevices(string userId);

    }
}
