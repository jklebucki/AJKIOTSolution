using MQTTnet;
using MQTTnet.Server;
using System.Text;

namespace AJKIOT.Api.Controllers
{
    public class MqttController
    {
        private readonly MqttServer _mqttServer;
        public MqttController(MqttServer mqttServer)
        {
            _mqttServer = mqttServer;
        }

        public async Task OnClientConnected(ClientConnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
            await Task.FromResult(true);
        }


        public async Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Accepting!");
            await Task.FromResult(true); // Task.CompletedTask;
        }

        public async Task OnInterceptingPublish(InterceptingPublishEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' {eventArgs.ApplicationMessage.Topic} {eventArgs.ApplicationMessage.PayloadSegment}");

            await Task.FromResult(true);
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
