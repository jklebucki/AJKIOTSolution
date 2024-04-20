using System;
using System.Collections.Concurrent;

namespace AJKIOT.Api.Hubs
{
    public class ConnectionMapping
    {
        private readonly ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

        public void Add(string connectionId, string client)
        {
            _connections.TryAdd(connectionId, client);
        }

        public IEnumerable<KeyValuePair<string,string>> GetAllClients()
        {
            return _connections;
        }

        public void Remove(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }
    }
}
