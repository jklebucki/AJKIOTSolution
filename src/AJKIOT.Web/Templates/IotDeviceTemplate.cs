using AJKIOT.Shared.Models;
using AJKIOT.Shared.Models.DeviceFeatures;
using System.Text.Json;

namespace AJKIOT.Web.Templates
{
    public class IotDeviceTemplate
    {
        public static IotDevice CreateDevice(DeviceTypeSelect deviceSelect)
        {

            switch (deviceSelect.Type)
            {
                case DeviceType.Switch:
                    return CreateSwitch();
                case DeviceType.OpenTimer:
                    return CreateOpenTimer();
                default:
                    return new IotDevice();
            }
        }

        private static IotDevice CreateSwitch()
        {
            return new IotDevice()
            {
                DeviceType = DeviceType.Switch.ToString(),
                DeviceName = "Switch",
                DeviceFeaturesJson = JsonSerializer.Serialize(new List<DeviceFeature> {
                    new DeviceFeature { Id = 1, Name = "Switch", Type = "Switch", Value = 0, MaxValue = 1 }
                })
            };
        }

        private static IotDevice CreateOpenTimer()
        {
            return new IotDevice()
            {
                DeviceType = DeviceType.OpenTimer.ToString(),
                DeviceName = "Open timer",
                DeviceFeaturesJson = JsonSerializer.Serialize(new List<DeviceFeature> {
                    new DeviceFeature { Id = 1, Name = "Open timer", Type = "Open timer", Value = 0, MaxValue = 1000 }
                })
            };
        }
    }
}