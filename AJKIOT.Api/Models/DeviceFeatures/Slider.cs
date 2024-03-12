namespace AJKIOT.Api.Models.DeviceFeatures
{
    public class Slider
    {
        public string? FeatureName { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int Value { get; set; }
    }
}
