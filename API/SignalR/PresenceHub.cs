using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker __tracker;
        public PresenceHub(PresenceTracker _tracker)
        {
            __tracker = _tracker;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var isOnline = await __tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOnline)
                // "UserIsOnline" is the client method that will be used to inform users someone has connected
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            // send the list cache of all online users
            var currentUsers = await __tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await __tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }
    }
}