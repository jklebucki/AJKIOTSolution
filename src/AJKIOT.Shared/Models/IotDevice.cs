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

        public bool IsScheduleAvailable()
        {
            switch (DeviceType)
            {
                case "Switch":
                    return true;
                case "OpenTimer":
                    return false;
                default:
                    return false;
            }
        }

        public void SetSchedule(IEnumerable<DailyScheduleEntry> dailyScheduleEntries)
        {
            if (IsScheduleAvailable() && dailyScheduleEntries != null)
                DeviceScheduleJson = JsonSerializer.Serialize(dailyScheduleEntries);
        }

        public IEnumerable<DailyScheduleEntry> GetSchedule()
        {
            try
            {
                if (IsScheduleAvailable() && !string.IsNullOrEmpty(DeviceScheduleJson))
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