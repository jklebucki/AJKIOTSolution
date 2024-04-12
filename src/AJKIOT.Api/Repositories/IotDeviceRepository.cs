using AJKIOT.Api.Data;
using AJKIOT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AJKIOT.Api.Repositories
{
    public class IotDeviceRepository : IIotDeviceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IotDeviceRepository> _logger;
        public IotDeviceRepository(ApplicationDbContext context, ILogger<IotDeviceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> AddDeviceAsync(IotDevice device)
        {
            var data = _context.IotDevices.Add(device);
            await _context.SaveChangesAsync();
            return data.Entity.Id;
        }

        public async Task<bool> DeleteDeviceAsync(int id)
        {
            try
            {
                var device = await _context.IotDevices.FirstOrDefaultAsync(x => x.Id == id);
                if (device != null)
                {
                    _context.IotDevices.Remove(device);
                    await _context.SaveChangesAsync();
                    return true;
                }
                throw new Exception("Device not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<IotDevice> GetDeviceAsync(string userId, int deviceId)
        {
            var device = await _context.IotDevices.FirstOrDefaultAsync(x => x.OwnerId == userId && x.Id == deviceId);
            return device!;
        }

        public async Task<IEnumerable<IotDevice>> GetUserDevicesAsync(string userId)
        {
            var devices = await _context.IotDevices.Where(x => x.OwnerId == userId).ToListAsync();
            return devices;
        }

        public async Task<IotDevice> UpdateDeviceAsync(IotDevice device)
        {
            _context.IotDevices.Update(device);
            await _context.SaveChangesAsync();
            return await Task.FromResult(device);
        }
    }
}
