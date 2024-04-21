using AJKIOT.Shared.Enums;

namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class DeviceFeature
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int NumberOfSteps { get; set; }

        public bool IsScheduleAvailable()
        {
            var scheduledTypes = Enum.GetNames(typeof(ScheduledFeature)).ToList();
            if (scheduledTypes is null)
                return false;
            return scheduledTypes.Contains(Type);
        }
        public void Update(DeviceFeature deviceFeature)
        {
            Type = deviceFeature.Type;
            Name = deviceFeature.Name;
            Value = deviceFeature.Value;
            MinValue = deviceFeature.MinValue;
            MaxValue = deviceFeature.MaxValue;
            NumberOfSteps = deviceFeature.NumberOfSteps;
        }
    }
}
