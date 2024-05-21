using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJKIOT.Shared.Models
{
    public class IotDeviceStatus
    {
        public int DeviceId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastOnlineSignal { get; set; }
    }
}
