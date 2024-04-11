using AJKIOT.Shared.Models;

namespace AJKIOT.Shared.Requests
{
    public class UpdateDeviceRequest
    {
        public IotDevice Device { get; set; } = new IotDevice();
    }
}