using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Shared.Models
{
    public class IotDevice
    {
        [Key]
        public int Id { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceFeaturesJson { get; set; } = string.Empty;
        public string DeviceSheduleJson { get; set; } = string.Empty;
        public bool IsScheduleAvailable()
        {
            switch (DeviceType)
            {
                case "Switch":
                    return true;
                case "OpenTimer":
                    return false;
                default:
                    return false;
            }
        }
    }
}