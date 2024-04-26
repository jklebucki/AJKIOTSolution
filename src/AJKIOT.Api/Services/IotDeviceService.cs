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

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(int id)
        {
            bool deleted = await _repository.DeleteDeviceAsync(id);
            return new ApiResponse<bool>() { Data = deleted };
        }

        public async Task<IotDevice> GetDeviceAsync(int devdeviceId)
        {
            return await _repository.GetDeviceAsync(devdeviceId);
        }

        public async Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string userId)
        {
            var devices = await _repository.GetUserDevicesAsync(userId);
            return new ApiResponse<IEnumerable<IotDevice>>() { Data = devices.OrderBy(d => d.Id) };
        }

        public async Task<ApiResponse<IotDevice>> UpdateDeviceAsync(IotDevice iotDevice)
        {
            var device = await _repository.UpdateDeviceAsync(iotDevice);
            return new ApiResponse<IotDevice>() { Data = device };
        }
    }
}
