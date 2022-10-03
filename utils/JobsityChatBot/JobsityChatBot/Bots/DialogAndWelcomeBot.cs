using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityChatBot.Bots
{
    /// <summary>
    /// Welcome and bot commands
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        /// <summary>
        /// Messages for user's welcome
        /// </summary>
        /// <param name="membersAdded"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (IMessageActivity response in from ChannelAccount member in membersAdded
                where member.Id != turnContext.Activity.Recipient.Id
                let welcomeCard = CreateAdaptiveCardAttachment()
                let response = MessageFactory.Attachment(welcomeCard, ssml: "Welcome to Jobsity Chat!")
                select response)
            {
                await turnContext.SendActivityAsync(response, cancellationToken);
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
        }

        /// <summary>
        /// Load attachment from embedded resource.
        /// </summary>
        /// <returns></returns>
        private Attachment CreateAdaptiveCardAttachment()
        {
            string cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));
            using Stream stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath);
            using StreamReader reader = new(stream);
            string adaptiveCard = reader.ReadToEnd();
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard, new JsonSerializerSettings { MaxDepth = null }),
            };
        }
    }
}
