using JobsityChatAngular.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace JobsityChatAngular.Hubs
{
    public class JobsityChatHub : Hub
    {
        public async Task JoinGroup(string groupName, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("NewUser", $"{userName} entró al canal");
        }

        public async Task LeaveGroup(string groupName, string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("LeftUser", $"{userName} salió del canal");
        }

        public async Task SendMessage(NewMessage message)
        {
            if (message.Message.Trim().StartsWith("/stock="))
            {
                JobsityChatBotHelper.ProcessMessage(message.Message);
            }
            await Clients.Group(message.GroupName).SendAsync("NewMessage", message);
        }

        public record NewMessage(DateTime date, string UserName, string Message, string GroupName);
    }
}
