using Azure.Messaging.ServiceBus;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityChatBot.Dialogs
{
    /// <summary>
    /// Bot Dialog to ask stocks from an Azure Service Bus queue
    /// </summary>
    public class StockDialog : ComponentDialog
    {
        private readonly IConfiguration configuration;

        public StockDialog(IConfiguration configuration)
            : base(nameof(StockDialog))
        {
            this.configuration = configuration;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                ReadStockFromServiceBusAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Ask an Azure Service Bus queue for any message
        /// </summary>
        /// <param name="stepContext">Current steps of the Dialog</param>
        /// <param name="cancellationToken">Token for cancellation between threads</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ReadStockFromServiceBusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string connString = configuration.GetValue<string>("azureServiceResponse");
            string queueName = configuration.GetValue<string>("queueName");
            await using ServiceBusClient client = new(connString);
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);

            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
                string body = receivedMessage.Body.ToString();
                Activity message = MessageFactory.Text(body, body, InputHints.IgnoringInput);
                return await stepContext.EndDialogAsync(message, cancellationToken);
            }
            finally
            {
                await client.DisposeAsync();
            }
        }
    }
}
