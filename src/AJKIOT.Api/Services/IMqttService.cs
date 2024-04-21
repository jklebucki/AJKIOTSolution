namespace AJKIOT.Api.Services
{
    public interface IMqttService
    {
        Task StartMqttServerAsync();
        Task PublishMessageAsync(string topic, string payload);
    }
}
