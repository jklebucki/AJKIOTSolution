using AJKIOT.Shared.Models.DeviceFeatures;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AJKIOT.Shared.Models
{
    public class IotDevice
    {
        [Key]
        public int Id { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceFeaturesJson { get; set; } = string.Empty;
        public string DeviceScheduleJson { get; set; } = string.Empty;


        public void SetFeatures(IEnumerable<DeviceFeature> deviceFeatures)
        {
            if (deviceFeatures != null)
                DeviceFeaturesJson = JsonSerializer.Serialize(deviceFeatures);
        }


        public IEnumerable<DeviceFeature> GetFeatures()
        {
            try
            {
                if (!string.IsNullOrEmpty(DeviceFeaturesJson))
                    return JsonSerializer.Deserialize<IEnumerable<DeviceFeature>>(DeviceFeaturesJson)!;
                else
                    return new List<DeviceFeature>();
            }
            catch
            {
                return new List<DeviceFeature>();
            }
        }

        public void SetSchedule(IEnumerable<DailyScheduleEntry> dailyScheduleEntries)
        {
            if (dailyScheduleEntries != null)
                DeviceScheduleJson = JsonSerializer.Serialize(dailyScheduleEntries);
        }

        public IEnumerable<DailyScheduleEntry> GetSchedule()
        {
            try
            {
                if (!string.IsNullOrEmpty(DeviceScheduleJson))
                    return JsonSerializer.Deserialize<IEnumerable<DailyScheduleEntry>>(DeviceScheduleJson)!;
                else
                    return new List<DailyScheduleEntry>();
            }
            catch
            {
                return new List<DailyScheduleEntry>();
            }
        }
    }
}