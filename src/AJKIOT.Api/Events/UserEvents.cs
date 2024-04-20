using System;
using AJKIOT.Api.Services;
using static AJKIOT.Api.Services.UserService;

namespace AJKIOT.Api.Events
{
    public class UserEvents
    {
        public static void UserService_UserDeleted(object sender, UserEventArgs e)
        {
            // Remove all user devices & shares
            var service = (UserService)sender;
            
            Console.WriteLine($"User {e.User!.UserName} has been deleted.");
        }
    }
}
