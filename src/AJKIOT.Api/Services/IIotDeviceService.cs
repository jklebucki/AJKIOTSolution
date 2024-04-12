﻿using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public interface IIotDeviceService
    {
        Task<ApiResponse<IotDevice>> AddDeviceAsync(IotDevice iotDevice);
        Task<ApiResponse<bool>> DeleteDeviceAsync(int deviceId);
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string userId);
        Task<ApiResponse<IotDevice>> UpdateDeviceAsync(IotDevice iotDevice);
    }
}
