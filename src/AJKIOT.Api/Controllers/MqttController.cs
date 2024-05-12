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
            _mqttServer.ClientDisconnectedAsync += OnClientDisconnected;
        }

        public async Task OnClientConnected(ClientConnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
            await Task.FromResult(true);
        }

        public async Task OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' disconnected. Reason: '{eventArgs.ReasonString}'.");
            await Task.FromResult(true);
        }

        public async Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
        {
            string[] allowedClients = { "device-0000", "device-0001", "3" };
            if (!allowedClients.Contains(eventArgs.ClientId))
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Not accepting!");
                await eventArgs.ChannelAdapter.DisconnectAsync(CancellationToken.None);
            }
            Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Accepting!");
            await Task.FromResult(true);
        }

        public async Task OnInterceptingPublish(InterceptingPublishEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' {eventArgs.ApplicationMessage.Topic} {Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment)}");
            await Task.FromResult(true);
        }

        public async Task PublishMessageAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "AJKIOT.MQTT.Server",
            });
        }
    }
}
