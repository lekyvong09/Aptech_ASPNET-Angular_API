using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            await Clients.Others.SendAsync("UserIsOnline", username);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            await Clients.Others.SendAsync("UserIsOffline", username);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
