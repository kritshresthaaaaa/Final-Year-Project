using Microsoft.AspNetCore.SignalR;

namespace FypWeb
{
    public class RfidHub : Hub
    {
        public async Task SendRfidTag(string tag)
        {
            await Clients.All.SendAsync("ReceiveRfidTag", tag);
        }
    }
}