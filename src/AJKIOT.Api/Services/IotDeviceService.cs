using AJKIOT.Api.Repositories;
using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public class IotDeviceService : IIotDeviceService
    {
        private readonly IIotDeviceRepository _repository;

        public IotDeviceService(IIotDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<IotDevice>> AddDeviceAsync(IotDevice iotDevice)
        {
            var deviceId = await _repository.AddDeviceAsync(iotDevice);
            iotDevice.Id = deviceId;
            if (deviceId == 0)
                return new ApiResponse<IotDevice>() { Data = iotDevice, Errors = new List<string> { "Error adding device" } };
            else
                return new ApiResponse<IotDevice>() { Data = iotDevice, Errors = new List<string>() };
        }

        public Task<ApiResponse<IotDevice>> DeleteDeviceAsync(IotDevice iotDevice)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string userId)
        {
            var devices = _repository.GetUserDevicesAsync(userId);
            return new ApiResponse<IEnumerable<IotDevice>>() { Data = await devices };
        }

        public Task<ApiResponse<IotDevice>> UpdateDeviceAsync(IotDevice iotDevice)
        {
            throw new NotImplementedException();
        }
    }
}
