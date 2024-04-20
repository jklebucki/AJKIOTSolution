using AJKIOT.Api.Services;
using static AJKIOT.Api.Services.UserService;

namespace AJKIOT.Api.Events
{
    public class UserEvents
    {
        public static void UserService_UserDeleted(object sender, UserEventArgs e)
        {
            // Remove all user devices & shares
            var deviceService = (IIotDeviceService)sender;
            var userDevices = deviceService.GetUserDevicesAsync(e.User!.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            if (userDevices.IsSuccess && userDevices.Data != null)
                foreach (var device in userDevices.Data)
                {
                    deviceService.DeleteDeviceAsync(device.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            Console.WriteLine($"User {e.User!.UserName} has been deleted.");
        }
    }
}
