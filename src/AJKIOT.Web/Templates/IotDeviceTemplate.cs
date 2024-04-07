using AJKIOT.Shared.Models;
using AJKIOT.Shared.Models.DeviceFeatures;
using System.Text.Json;

namespace AJKIOT.Web.Templates
{
    public class IotDeviceTemplate
    {
        public static IotDevice CreateDevice(DeviceType deviceType)
        {

            switch (deviceType.Name)
            {
                case "Switch":
                    return CreateSwitch();
                case "Open timer":
                    return CreateOnTimer();
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

        private static IotDevice CreateOnTimer()
        {
            return new IotDevice()
            {
                DeviceType = "Open timer",
                DeviceName = "Open timer",
                DeviceFeaturesJson = JsonSerializer.Serialize(new List<DeviceFeature> {
                    new DeviceFeature { Id = 1, Name = "Open timer", Type = "Open timer", Value = 0, MaxValue = 1000 }
                })
            };
        }
    }
}