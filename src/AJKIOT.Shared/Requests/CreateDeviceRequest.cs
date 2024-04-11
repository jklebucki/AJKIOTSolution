using AJKIOT.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJKIOT.Shared.Requests
{
    public class CreateDeviceRequest
    {
        public string UserEmail { get; set; } = string.Empty;
        public IotDevice Device { get; set; } = new IotDevice();
    }
}
