namespace AJKIOT.Shared.Models
{
    public class IotDevice
    {
        public string? OwnerId { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = "";
        public string DeviceFeaturesJson { get; set; } = "{}";
        public string CurrentSettingsJson { get; set; } = "{}";
    }
}