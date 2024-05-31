using AJKIOT.Api.Hubs;
using AJKIOT.Api.Services;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Server;
using System.Text;
using System.Text.Json;

namespace AJKIOT.Api.Controllers
{
    public class MqttController
    {
        private readonly MqttServer _mqttServer;
        private readonly IDeviceData _deviceData;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly DeviceIdStore _deviceIdStore;
        public MqttController(MqttServer mqttServer, IHubContext<NotificationHub> hubContext, IDeviceData deviceData, DeviceIdStore deviceIdStore)
        {
            _mqttServer = mqttServer;
            _mqttServer.ClientDisconnectedAsync += OnClientDisconnected;
            _hubContext = hubContext;
            _deviceData = deviceData;
            _deviceIdStore = deviceIdStore;
        }

        public async Task OnClientConnected(ClientConnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' connected, protocol version: '{eventArgs.ProtocolVersion}'.");
            await Task.FromResult(true);
        }

        public async Task OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' disconnected. Reason: '{eventArgs.ReasonString}'.");
            await Task.FromResult(true);
        }

        public async Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
        {
            var allowedClients = _deviceIdStore.GetAllDeviceIds().ToList();
            allowedClients.Add( "device-0000");  //for testing
            allowedClients.Add("device-0001");  //for testing
            if (!allowedClients.Contains(eventArgs.ClientId))
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Not accepting!");
                await eventArgs.ChannelAdapter.DisconnectAsync(CancellationToken.None);
            }
            else
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Accepting!");
            }
            await Task.FromResult(true);
        }

        public async Task OnInterceptingPublish(InterceptingPublishEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' {eventArgs.ApplicationMessage.Topic} {Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment)}");
            if (eventArgs.ApplicationMessage.Topic == $"controlDevice/{eventArgs.ClientId}")
                await _hubContext.Clients.All.SendAsync("ControlSignal", Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment));
            if (eventArgs.ApplicationMessage.Topic == $"configDevice/{eventArgs.ClientId}")
            {
                var device = await _deviceData.GetDeviceAsync(int.Parse(Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment)));
                await PublishMessageAsync($"updateFeature/{eventArgs.ClientId}", JsonSerializer.Serialize(device.GetFeatures().ToList()[0]));
                await PublishMessageAsync($"signalSchedule/{eventArgs.ClientId}", "start");
                foreach (var shedule in device.GetSchedule())
                {
                    await PublishMessageAsync($"configSchedule/{eventArgs.ClientId}", JsonSerializer.Serialize(shedule));
                }
                await PublishMessageAsync($"signalSchedule/{eventArgs.ClientId}", "stop");
            }
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
