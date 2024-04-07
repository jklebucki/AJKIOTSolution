namespace AJKIOT.Shared.Models
{
    public class IotDevice
    {
        public string? OwnerId { get; set; }
        public int DeviceId { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string DeviceName { get; set; } = "";
        public string DeviceFeaturesJson { get; set; } = "{}";
    }
}