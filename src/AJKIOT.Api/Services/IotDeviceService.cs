using AJKIOT.Api.Repositories;
using AJKIOT.Shared.Models;
using AJKIOT.Shared.Settings;
using System.IO.Compression;

namespace AJKIOT.Api.Services
{
    public class IotDeviceService : IIotDeviceService
    {
        private readonly IIotDeviceRepository _repository;
        private readonly ILogger _logger;
        private readonly DeviceIdStore _deviceIdStore;

        public IotDeviceService(IIotDeviceRepository repository, ILogger<IotDeviceService> logger, DeviceIdStore deviceIdStore)
        {
            _repository = repository;
            _logger = logger;
            _deviceIdStore = deviceIdStore;
        }

        public async Task<ApiResponse<IotDevice>> AddDeviceAsync(IotDevice iotDevice)
        {
            var deviceId = await _repository.AddDeviceAsync(iotDevice);
            iotDevice.Id = deviceId;
            if (deviceId == 0)
                return new ApiResponse<IotDevice>() { Data = iotDevice, Errors = new List<string> { "Error adding device" } };
            else
            {
                _deviceIdStore.AddDeviceId(deviceId.ToString());
                return new ApiResponse<IotDevice>() { Data = iotDevice, Errors = new List<string>() };
            }

        }

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(int id)
        {
            bool deleted = await _repository.DeleteDeviceAsync(id);
            if (deleted)
                _deviceIdStore.RemoveDeviceId(id.ToString());
            return new ApiResponse<bool>() { Data = deleted };
        }

        public async Task<IotDevice> GetDeviceAsync(int devdeviceId)
        {
            return await _repository.GetDeviceAsync(devdeviceId);
        }

        public async Task<Stream> GetDeviceFirmwareAsStreamAsync(FirmwareSettings firmwareSettings)
        {
            var deviceType = firmwareSettings.DeviceType;
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "FirmwareFiles", deviceType);
                var files = Directory.GetFiles(folderPath);
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);
                    using (var fileStream = File.OpenRead(file))
                    using (var entryStream = entry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
            memoryStream.Position = 0;
            return memoryStream;
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

        public async IAsyncEnumerable<string> GetAllowedDevicesAsync()
        {
            var devices = (await _repository.GetAllDevicesAsync()).Select(d => d.Id.ToString());
            await foreach (var deviceId in devices.ToAsyncEnumerable())
            {
                yield return deviceId;
            }
        }
    }
}
