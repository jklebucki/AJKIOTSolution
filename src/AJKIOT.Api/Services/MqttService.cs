using MQTTnet;
using MQTTnet.Server;
using System.Text;

namespace AJKIOT.Api.Services
{
    public class MqttService : IMqttService
    {
        private MqttServer _mqttServer;

        public MqttService()
        {
            var factory = new MqttFactory();
            var optionsBuilder = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
            _mqttServer = factory.CreateMqttServer(optionsBuilder);
        }

        public async Task StartMqttServerAsync()
        {

            await _mqttServer.StartAsync();
        }

        public async Task PublishMessageAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();


            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "SenderClientId"
            });
        }
    }
}
