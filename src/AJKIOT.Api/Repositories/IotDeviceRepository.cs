using AJKIOT.Api.Data;
using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Repositories
{
    public class IotDeviceRepository : IIotDeviceRepository
    {
        private readonly ApplicationDbContext _context;
        public IotDeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddDeviceAsync(IotDevice device)
        {
            var data = _context.IotDevices.Add(device);
            await _context.SaveChangesAsync();
            return data.Entity.Id;
        }

        public Task<IotDevice> GetDeviceAsync(string userId, string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IotDevice>> GetUserDevicesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IotDevice> UpdateDeviceAsync(IotDevice device)
        {
            throw new NotImplementedException();
        }
    }
}
