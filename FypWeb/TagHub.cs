using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class TagHub : Hub
{
    public async Task SendTagData(string tag, int count)
    {
        await Clients.All.SendAsync("ReceiveTagData", tag, count);
    }
}
