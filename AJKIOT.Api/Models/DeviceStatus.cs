using System;

namespace AJKIOT.Api.Models
{
    public class DeviceStatus
    {
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public int PinStatus { get; set; }
        public int SetPinStatus { get; set; }

    }
}
