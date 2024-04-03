using System;
using System.Text.Json;
using AJKIOT.Shared.Models;
using AJKIOT.Shared.Models.DeviceFeatures;

namespace AJKIOT.Api.Data
{
    public static class DevicesTestData
    {
        public static List<IotDevice> Devices ()
        {
            var features = JsonSerializer.Serialize(new List<DeviceFeature>{
                                new DeviceFeature { Id = 1, Name = "DigitalOutput0", Type = "DigitalOutput" }, 
                                new DeviceFeature { Id = 2, Name = "DigitalOutput1", Type = "DigitalOutput" }});
            return new List<IotDevice>
                    {                       
                        new IotDevice { 
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", 
                            DeviceId = 1, DeviceName = "Water Leak Sensor #1", 
                            CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", 
                            DeviceFeaturesJson = features},
                        new IotDevice { 
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", 
                            DeviceId = 2, DeviceName = "Security Camera #2", 
                            CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"1\"}",
                            DeviceFeaturesJson = features},
                        new IotDevice { 
                            OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", 
                            DeviceId = 3, DeviceName = "Security Camera #3", 
                            CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", 
                            DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 4, DeviceName = "Smoke Detector #4", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 5, DeviceName = "Smoke Detector #5", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"1\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 6, DeviceName = "Water Leak Sensor #6", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 7, DeviceName = "Smart Lock #7", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 8, DeviceName = "Light Bulb #8", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 9, DeviceName = "Smoke Detector #9", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "62ad23c4-20a2-4c14-893c-7d7abd3e468b", DeviceId = 10, DeviceName = "Light Bulb #10", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"1\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 11, DeviceName = "Smart Lock #1", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 12, DeviceName = "Water Leak Sensor #2", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 13, DeviceName = "Security Camera #3", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 14, DeviceName = "Smoke Detector #4", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 15, DeviceName = "Security Camera #5", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 16, DeviceName = "Light Bulb #6", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "890529a7-f981-4b67-a970-a4c82059f324", DeviceId = 17, DeviceName = "Thermostat #7", CurrentSettingsJson="{\"DigitalOutput0\":\"1\",\"DigitalOutput1\":\"1\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", DeviceId = 18, DeviceName = "Water Leak Sensor #1", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", DeviceId = 19, DeviceName = "Thermostat #2", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", DeviceId = 20, DeviceName = "Smoke Detector #3", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", DeviceId = 21, DeviceName = "Thermostat #4", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"1\"}", DeviceFeaturesJson = features},
                        new IotDevice { OwnerId = "913b2815-1a16-489e-bec4-672f3e3f46dc", DeviceId = 22, DeviceName = "Thermostat #5", CurrentSettingsJson="{\"DigitalOutput0\":\"0\",\"DigitalOutput1\":\"0\"}", DeviceFeaturesJson = features},
                    };
        }

    }
}

