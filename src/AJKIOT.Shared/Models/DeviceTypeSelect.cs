namespace AJKIOT.Shared.Models
{
    public enum DeviceType
    {
        Switch = 1,
        OpenTimer,
    }

    public class DeviceTypeSelect
    {
        public int Id { get; set; }
        public DeviceType Type { get; set; }
    }
}
