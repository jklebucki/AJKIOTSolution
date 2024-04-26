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

        public IotDeviceService(IIotDeviceRepository repository, ILogger<IotDeviceService> logger)
        {
            _repository = repository;
            _logger = logger;
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

        private async Task<bool> InjectDeviceFirmwareSettingsAsync(FirmwareSettings firmwareSettings)
        {
            var settingsTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "FirmwareFiles", "FirmwareSettings.txt");
            var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "FirmwareFiles", firmwareSettings.DeviceType, "FirmwareSettings.txt");
            try
            {
                using (var sr = new StreamReader(settingsTemplatePath))
                {
                    var firmware = await sr.ReadToEndAsync();
                    if (firmware != null)
                    {
                        firmware = firmware.Replace(":ssid", firmwareSettings.WiFiSSID);
                        firmware = firmware.Replace(":password", firmwareSettings.WiFiPassword);
                        firmware = firmware.Replace(":apiUrl", firmwareSettings.ApiUrl);
                        File.WriteAllText(settingsPath, firmware);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error injecting device firmware settings: {ex.Message}");
                return false;
            }
        }

        public async Task<Stream> GetDeviceFirmwareAsStreamAsync(FirmwareSettings firmwareSettings)
        {
            if (await InjectDeviceFirmwareSettingsAsync(firmwareSettings))
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
            throw new Exception("Error injecting device firmware settings");
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
