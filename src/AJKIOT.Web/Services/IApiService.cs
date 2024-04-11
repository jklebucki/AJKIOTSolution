﻿using AJKIOT.Shared.Models;
using AJKIOT.Shared.Requests;

namespace AJKIOT.Web.Services
{
    public interface IApiService
    {
        Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string username);
        Task<ApiResponse<IotDevice>> CreateUserDeviceAsync(CreateDeviceRequest createDeviceRequest);
        Task<ApiResponse<IotDevice>> UpdateDeviceAsync(UpdateDeviceRequest updateDeviceRequest);

    }
}
