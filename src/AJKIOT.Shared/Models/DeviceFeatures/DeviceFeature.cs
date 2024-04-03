namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class DeviceFeature
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public int MaxRange { get; set; }
        public int StepsNumber { get; set; }

    }
}
