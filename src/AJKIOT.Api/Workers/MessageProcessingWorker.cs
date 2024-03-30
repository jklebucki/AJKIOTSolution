using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using System.Text.Json;

namespace AJKIOT.Api.Workers
{
    public class MessageProcessingWorker : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageProcessingWorker> _logger;
        private readonly IDocumentRepositoryFactory _documentRepositoryFactory;

        public MessageProcessingWorker(IMessageBus messageBus, ILogger<MessageProcessingWorker> logger, IDocumentRepositoryFactory documentRepositoryFactory)
        {
            _messageBus = messageBus;
            _logger = logger;
            _documentRepositoryFactory = documentRepositoryFactory;
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
                        string processedMessage = ProcessIncomingMessage(incomingMessage);
                        _messageBus.EnqueueMessage(processedMessage);

                        _logger.LogInformation($"Processed and enqueued message: {processedMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages.");
                }

                await Task.Delay(10, stoppingToken);
            }
        }

        private string ProcessIncomingMessage(string incomingMessage)
        {
            var messageJson = JsonDocument.Parse(incomingMessage).RootElement;
            var id = messageJson.GetProperty("id").GetString();

            // Zmiana typu wiadomości na "out" i zwrócenie jej w formacie JSON.
            var outgoingMessage = new
            {
                id = id,
                type = "out",
                content = "Processed content" 
            };
            var documentRepository = _documentRepositoryFactory.CreateDocumentRepository();
            documentRepository.CreateAsync(new MongoDB.Bson.BsonDocument
            {
                { "id", id },
                { "type", "out" },
                { "content", "Processed content" }

            });
            return JsonSerializer.Serialize(outgoingMessage);
        }
    }
}
