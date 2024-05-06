namespace AJKIOT.Shared.Models
{
    public class MqttMessage
    {
        public int DeviceId { get; set; }
        public MqttMessageType MessageType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public enum MqttMessageType
    {
        UpdateFeature,
        Config,
        Control,
        UpdateSchedule
    }
}
