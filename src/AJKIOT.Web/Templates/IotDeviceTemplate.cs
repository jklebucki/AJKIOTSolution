using AJKIOT.Shared.Models;
using AJKIOT.Shared.Models.DeviceFeatures;
using System.Text.Json;

namespace AJKIOT.Web.Templates
{
    public class IotDeviceTemplate
    {
        public static IotDevice CreateDevice(string deviceSelect)
        {

            switch (deviceSelect)
            {
                case "Switch":
                    return CreateSwitch();
                case "OpenTimer":
                    return CreateOpenTimer();
                default:
                    return new IotDevice();
            }
        }

        private static IotDevice CreateSwitch()
        {
            return new IotDevice()
            {
                DeviceType = "Switch",
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
                DeviceType = "OpenTimer",
                DeviceName = "Open timer",
                DeviceFeaturesJson = JsonSerializer.Serialize(new List<DeviceFeature> {
                    new DeviceFeature { Id = 1, Name = "Open timer", Type = "OpenTimer", Value = 0, MaxValue = 1000 }
                })
            };
        }
    }
}