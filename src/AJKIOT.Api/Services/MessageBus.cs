using System.Collections.Concurrent;
using System.Text.Json;

namespace AJKIOT.Api.Services
{
    public class MessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskCompletionSource<string>>> _responseQueues = new ConcurrentDictionary<string, ConcurrentQueue<TaskCompletionSource<string>>>();
        private readonly ILogger<MessageBus> _logger;

        public MessageBus(ILogger<MessageBus> logger)
        {
            _logger = logger;
        }

        public Task<string> SendMessageAsync(string message)
        {
            // Check if the message is null or empty
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("SendMessageAsync was called with a null or empty message.");
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }

            // Attempt to deserialize the message and extract the 'id' property
            var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
            if (messageObj.ValueKind == JsonValueKind.Undefined || !messageObj.TryGetProperty("id", out var idProperty) || idProperty.GetString() is not string id || string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("SendMessageAsync was called with a message that lacks a non-empty 'id' property.");
                return Task.FromResult<string>(string.Empty); // Return empty string to avoid throwing an exception
            }

            var tcs = new TaskCompletionSource<string>();
            var queue = _responseQueues.GetOrAdd(id, _ => new ConcurrentQueue<TaskCompletionSource<string>>());
            queue.Enqueue(tcs);

            _logger.LogInformation($"A message has been queued for id {id}.");
            return tcs.Task;
        }

        public void ReceiveMessage(string message)
        {
            // Check if the received message is null or empty
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("Received message cannot be null or empty.");
                return;
            }

            // Attempt to deserialize the received message and extract the 'id' property
            var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
            if (messageObj.ValueKind == JsonValueKind.Undefined || !messageObj.TryGetProperty("id", out var idProperty) || idProperty.GetString() is not string id || string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("The received message does not contain a non-empty 'id'.");
                return;
            }

            // Try to deliver the message to the corresponding waiting task
            if (_responseQueues.TryGetValue(id, out var queue) && queue.TryDequeue(out var tcs))
            {
                tcs.SetResult(message); // Set the received message as the result of the waiting task
                _logger.LogInformation($"A message has been delivered for id {id}.");
            }
            else
            {
                _logger.LogWarning($"No waiting tasks found for the message with id '{id}'.");
            }
        }
    }
}
