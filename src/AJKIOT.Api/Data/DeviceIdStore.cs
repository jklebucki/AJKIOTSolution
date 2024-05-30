using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DeviceIdStore
{
    private readonly ConcurrentDictionary<string, bool> _deviceIds;

    public DeviceIdStore()
    {
        _deviceIds = new ConcurrentDictionary<string, bool>();
    }

    public void LoadDeviceIds(IEnumerable<string> deviceIds)
    {
        _deviceIds.Clear();
        foreach (var id in deviceIds)
        {
            _deviceIds[id] = true;
        }
    }

    public bool Contains(string deviceId)
    {
        return _deviceIds.ContainsKey(deviceId);
    }

    public void AddDeviceId(string deviceId)
    {
        _deviceIds.TryAdd(deviceId, true);
    }

    public void RemoveDeviceId(string deviceId)
    {
        _deviceIds.TryRemove(deviceId, out _);
    }

    public async Task LoadDeviceIdsAsync(IAsyncEnumerable<string> deviceIds)
    {
        _deviceIds.Clear();
        await foreach (var id in deviceIds)
        {
            _deviceIds[id] = true;
        }
    }

    public IEnumerable<string> GetAllDeviceIds()
    {
        return _deviceIds.Keys;
    }
}
