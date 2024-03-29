using AJKIOT.Api.Services;
using System.Text.Json;

namespace AJKIOT.Api.Workers
{
    public class MessageProcessingWorker : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageProcessingWorker> _logger;

        public MessageProcessingWorker(IMessageBus messageBus, ILogger<MessageProcessingWorker> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
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
                        // Przetwarzanie przychodzącej wiadomości i zmiana jej stanu
                        string processedMessage = ProcessIncomingMessage(incomingMessage);

                        // Zapisanie przetworzonej wiadomości jako typu "out"
                        _messageBus.EnqueueMessage(processedMessage);

                        _logger.LogInformation($"Processed and enqueued message: {processedMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages.");
                }

                // Oczekiwanie na kolejną wiadomość
                await Task.Delay(10, stoppingToken);
            }
        }

        private string ProcessIncomingMessage(string incomingMessage)
        {
            // Tutaj umieść logikę przetwarzania wiadomości. Poniżej przykład zmiany typu na "out".
            var messageJson = JsonDocument.Parse(incomingMessage).RootElement;
            var id = messageJson.GetProperty("id").GetString();

            // Zmiana typu wiadomości na "out" i zwrócenie jej w formacie JSON.
            var outgoingMessage = new
            {
                id = id,
                type = "out",
                content = "Processed content" // Przykładowa zmiana treści
            };

            return JsonSerializer.Serialize(outgoingMessage);
        }
    }
}
