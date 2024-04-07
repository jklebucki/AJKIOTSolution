using AJKIOT.Api.Middleware;
using AJKIOT.Api.Services;
using System.Text.Json;

namespace AJKIOT.Api.Workers
{
    public class MessageProcessingWorker : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageProcessingWorker> _logger;
        private readonly IWebSocketManager _webSocketManager;

        public MessageProcessingWorker(IMessageBus messageBus, ILogger<MessageProcessingWorker> logger, IWebSocketManager webSocketManager)
        {
            _messageBus = messageBus;
            _logger = logger;
            _webSocketManager = webSocketManager;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)

        {
            _logger.LogInformation("Message Processing Worker running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string incomingMessage = await _messageBus.GetNextIncomingMessageAsync();
                    if (!string.IsNullOrEmpty(incomingMessage))
                    {
                        string processedMessage = await ProcessIncomingMessage(incomingMessage);
                        if (!string.IsNullOrEmpty(processedMessage))
                        {
                            var messageJson = JsonDocument.Parse(processedMessage).RootElement;
                            var id = messageJson.GetProperty("_id").GetString();
                            await _webSocketManager.SendMessageToClientAsync(id!, processedMessage);
                            _logger.LogInformation($"Processed and enqueued message: {processedMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages.");
                }

                await Task.Delay(5, stoppingToken);
            }
        }

        private async Task<string> ProcessIncomingMessage(string incomingMessage)
        {
            var messageJson = JsonDocument.Parse(incomingMessage).RootElement;
            var id = messageJson.GetProperty("_id").GetString();
            var content = messageJson.GetProperty("content").GetString();
            var direction = messageJson.GetProperty("direction").GetString();
            var type = messageJson.GetProperty("type").GetString();

            if (type == "set")
            {
                var deviceProperties = messageJson.GetProperty("device_properties").ToString();
                var outgoingMessage = new
                {
                    _id = id,
                    direction = "out",
                    type = "set",
                    content = $"Processed content: {content}",
                    deviceProperties
                };
                return await Task.FromResult(JsonSerializer.Serialize(outgoingMessage));
            }
            else if (type == "query")
            {
                var deviceProperties = messageJson.GetProperty("device_properties").ToString();
                var outgoingMessage = new
                {
                    _id = id,
                    direction = "out",
                    type = "set",
                    content = $"Processed content: {content}",
                    deviceProperties
                };
                return await Task.FromResult(JsonSerializer.Serialize(outgoingMessage));
            }
            else
            {
                var outgoingMessage = new
                {
                    _id = id,
                    direction = "out",
                    type = "info",
                    content = $"Echo",
                };
                return await Task.FromResult(JsonSerializer.Serialize(outgoingMessage));
            }
        }
    }
}
