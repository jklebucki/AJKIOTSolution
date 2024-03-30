using System.Collections.Concurrent;
using System.Text.Json;

namespace AJKIOT.Api.Services
{
    public class MessageBus : IMessageBus
    {

        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _incomingMessages = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _outgoingMessages = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        private readonly ILogger<MessageBus> _logger;

        public MessageBus(ILogger<MessageBus> logger)
        {
            _logger = logger;
        }

        public void EnqueueMessage(string message)
        {
            var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
            var id = GetIdFromMessage(messageObj);
            var direction = GetDirectionFromMessage(messageObj);

            if (direction == "in")
            {
                var queue = _incomingMessages.GetOrAdd(id, _ => new ConcurrentQueue<string>());
                queue.Enqueue(message);
                _logger.LogInformation($"Enqueued an incoming message for id {id}.");
            }
            else if (direction == "out")
            {
                var queue = _outgoingMessages.GetOrAdd(id, _ => new ConcurrentQueue<string>());
                queue.Enqueue(message);
                _logger.LogInformation($"Enqueued an outgoing message for id {id}.");
            }
            else
            {
                _logger.LogError($"Message direction {direction} is not recognized.");
            }
        }

        public async Task<string> GetNextMessageAsync(string message, string direction)
        {
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(direction))
            {
                var messageRequest = JsonSerializer.Deserialize<JsonElement>(message);
                var id = GetIdFromMessage(messageRequest);

                if (direction == "in" && _incomingMessages.TryGetValue(id, out var inQueue) && inQueue.TryDequeue(out var inMessage))
                {
                    _logger.LogInformation($"Delivering an incoming message for id {id}.");
                    return inMessage;
                }
                else if (direction == "out" && _outgoingMessages.TryGetValue(id, out var outQueue) && outQueue.TryDequeue(out var outMessage))
                {
                    _logger.LogInformation($"Delivering an outgoing message for id {id}.");
                    return outMessage;
                }

                _logger.LogWarning($"No messages found for id {id} and direction {direction}.");
            }
            return await Task.FromResult<string>(string.Empty);
        }

        public async Task<string> GetNextIncomingMessageAsync()
        {
            foreach (var entry in _incomingMessages)
            {
                if (entry.Value.TryDequeue(out var message))
                {
                    _logger.LogInformation($"Delivering an incoming message for id {entry.Key}.");
                    return message;
                }
            }

            return await Task.FromResult<string>(string.Empty);
        }

        private string GetIdFromMessage(JsonElement messageObj)
        {
            if (!messageObj.TryGetProperty("_id", out var idProperty) || idProperty.GetString() is not string id || string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("Message must contain a non-empty 'id' property.");
                throw new InvalidOperationException("Message must contain a non-empty 'id' property.");
            }
            return id;
        }

        private string GetDirectionFromMessage(JsonElement messageObj)
        {
            if (!messageObj.TryGetProperty("direction", out var typeProperty) || typeProperty.GetString() is not string direction || string.IsNullOrWhiteSpace(direction))
            {
                _logger.LogError("Message must contain a 'direction' property.");
                throw new InvalidOperationException("Message must contain a 'type' property.");
            }
            return direction;
        }
    }
}
