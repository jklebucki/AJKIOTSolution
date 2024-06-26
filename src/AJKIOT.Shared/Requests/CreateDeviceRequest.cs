﻿using AJKIOT.Shared.Models;

namespace AJKIOT.Shared.Requests
{
    public class CreateDeviceRequest
    {
        public string UserEmail { get; set; } = string.Empty;
        public IotDevice Device { get; set; } = new IotDevice();
    }
}
