using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Services
{
    public class DeviceData : IDeviceData
    {
        private readonly IServiceProvider _serviceProvider;

        public DeviceData(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IotDevice> GetDeviceAsync(int deviceId)
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedService = scope.ServiceProvider.GetRequiredService<IIotDeviceService>();
            return await scopedService.GetDeviceAsync(deviceId);
        }
    }
}
