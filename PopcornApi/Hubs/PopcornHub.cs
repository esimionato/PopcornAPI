using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PopcornApi.Hubs
{
    public class PopcornHub : Hub
    {
        private int Users;

        public async Task BroadcastNumberOfUsers(int nbUser)
        {
            await Clients.All.InvokeAsync("OnUserConnected", nbUser);
        }

        public override async Task OnConnectedAsync()
        {
            Users++;
            await BroadcastNumberOfUsers(Users);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Users--;
            await BroadcastNumberOfUsers(Users);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
