﻿using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Repositories
{
    public interface IIotDeviceRepository
    {
        Task<int> AddDeviceAsync(IotDevice device);
        Task<IEnumerable<IotDevice>> GetUserDevicesAsync(string userId);
        Task<IotDevice> GetDeviceAsync(int deviceId);
        Task<IotDevice> UpdateDeviceAsync(IotDevice device);
        Task<bool> DeleteDeviceAsync(int id);
        Task<IEnumerable<IotDevice>> GetAllDevicesAsync();
    }
}
