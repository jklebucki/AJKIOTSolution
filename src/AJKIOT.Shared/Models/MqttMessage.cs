namespace AJKIOT.Shared.Models
{
    public class MqttMessage
    {
        public string Topic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
