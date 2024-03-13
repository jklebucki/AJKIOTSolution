namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class Knob
    {
        public string? FeatureName { get; set; }
        public bool IsStep { get; set; }
        public int? NumberOfSteps { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int Value { get; set; }
    }
}
