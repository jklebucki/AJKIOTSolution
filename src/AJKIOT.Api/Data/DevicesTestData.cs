using AJKIOT.Shared.Models;
using AJKIOT.Shared.Models.DeviceFeatures;
using System.Text.Json;

namespace AJKIOT.Api.Data
{
    public static class DevicesTestData
    {
        public static List<IotDevice> Devices()
        {
            var features = JsonSerializer.Serialize(new List<DeviceFeature>{
                                new DeviceFeature { Id = 1, Name = "DigitalOutput0", Type = "DigitalOutput" },
                                new DeviceFeature { Id = 2, Name = "DigitalOutput1", Type = "DigitalOutput" }});
            return new List<IotDevice>
                    {
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b",
                            Id = 1, DeviceName = "Water Leak Sensor #1", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 2, DeviceName = "Security Camera #2", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 3, DeviceName = "Security Camera #3", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 4, DeviceName = "Smoke Detector #4", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 5, DeviceName = "Smoke Detector #5", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 6, DeviceName = "Water Leak Sensor #6", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 7, DeviceName = "Smart Lock #7", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 8, DeviceName = "Light Bulb #8", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 9, DeviceName = "Smoke Detector #9", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", Id = 10, DeviceName = "Light Bulb #10", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 11, DeviceName = "Smart Lock #1",  DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 13, DeviceName = "Security Camera #3", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 14, DeviceName = "Smoke Detector #4", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 15, DeviceName = "Security Camera #5", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 16, DeviceName = "Light Bulb #6", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", Id = 17, DeviceName = "Thermostat #7", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", Id = 18, DeviceName = "Water Leak Sensor #1", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", Id = 19, DeviceName = "Thermostat #2", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", Id = 20, DeviceName = "Smoke Detector #3", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", Id = 21, DeviceName = "Thermostat #4", DeviceFeaturesJson = features},
                        new IotDevice {
                            OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", Id = 22, DeviceName = "Thermostat #5", DeviceFeaturesJson = features},
                    };
        }

    }
}

