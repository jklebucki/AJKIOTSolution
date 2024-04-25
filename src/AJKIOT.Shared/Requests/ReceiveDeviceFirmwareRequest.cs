namespace AJKIOT.Shared.Requests
{
    public class ReceiveDeviceFirmwareRequest
    {
        public string WiFiSSID { get; set; } = string.Empty;
        public string WiFiPassword { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public int DeviceId { get; set; }
    }
}
