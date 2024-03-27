namespace AJKIOT.Shared.Models
{
    public class IotDevice
    {
        public string? OwnerId { get; set; }
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public int PinStatus { get; set; }
        public int SetPinStatus { get; set; }
        public string? DeviceFeaturesJson { get; set; }

    }
}