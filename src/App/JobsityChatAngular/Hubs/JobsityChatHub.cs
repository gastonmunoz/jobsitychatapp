using JobsityChatAngular.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace JobsityChatAngular.Hubs
{
    /// <summary>
    /// Jobsity SignalR Hub for chat application
    /// </summary>
    public class JobsityChatHub : Hub
    {
        private readonly IConfiguration configuration;

        public JobsityChatHub(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Method executed when an user join into a chatroom
        /// </summary>
        /// <param name="groupName">Chatroom group</param>
        /// <param name="userName">User name</param>
        /// <returns></returns>
        public async Task JoinGroup(string groupName, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("NewUser", $"{userName} entró al canal");
        }

        /// <summary>
        /// Method executed when an user left from a chatroom
        /// </summary>
        /// <param name="groupName">Chatroom group</param>
        /// <param name="userName">User name</param>
        /// <returns></returns>
        public async Task LeaveGroup(string groupName, string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("LeftUser", $"{userName} salió del canal");
        }

        /// <summary>
        /// Middleware to process messages (from user and bot)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(NewMessage message)
        {
            if (message.Message.Trim().ToLowerInvariant().StartsWith("/stock="))
            {
                JobsityChatBotHelper.ProcessMessage(configuration, message.Message);
            }
            await Clients.Group(message.GroupName).SendAsync("NewMessage", message);
        }

        public record NewMessage(DateTime date, string UserName, string Message, string GroupName);
    }
}
