namespace AJKIOT.Shared.Models
{
    public class IotDeviceStatus
    {
        public int DeviceId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastOnlineSignal { get; set; }
    }
}
