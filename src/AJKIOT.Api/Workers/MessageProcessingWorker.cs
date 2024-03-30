using AJKIOT.Api.Middleware;
using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using MongoDB.Bson;
using System.Text.Json;

namespace AJKIOT.Api.Workers
{
    public class MessageProcessingWorker : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageProcessingWorker> _logger;
        private readonly IDocumentRepositoryFactory _documentRepositoryFactory;
        private readonly IWebSocketManager _webSocketManager;

        public MessageProcessingWorker(IMessageBus messageBus, ILogger<MessageProcessingWorker> logger, IDocumentRepositoryFactory documentRepositoryFactory, IWebSocketManager webSocketManager)
        {
            _messageBus = messageBus;
            _logger = logger;
            _documentRepositoryFactory = documentRepositoryFactory;
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
                            await _webSocketManager.SendMessageToClientAsync(id, processedMessage);
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

            var documentRepository = _documentRepositoryFactory.CreateDocumentRepository();
            if (type == "set")
            {
                var deviceProperties = messageJson.GetProperty("device_properties").ToString();
                await documentRepository.CreateOrUpdateAsync(new BsonDocument
                {
                    { "_id", id },
                    { "content", content },
                    { "device_properties", deviceProperties }
                });
                var outgoingMessage = new
                {
                    _id = id,
                    direction = "out",
                    type = "set",
                    content = $"Processed content: {content}",
                    deviceProperties
                };
                return JsonSerializer.Serialize(outgoingMessage);
            }
            else if (type == "query")
            {
                var document = await documentRepository.GetByIdAsync(id!);
                document.Add("direction", "out");
                if (document == null)
                    return string.Empty;
                var outgoingMessage = document.ToJson();
                return outgoingMessage;
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
                return JsonSerializer.Serialize(outgoingMessage);
            }
        }
    }
}
