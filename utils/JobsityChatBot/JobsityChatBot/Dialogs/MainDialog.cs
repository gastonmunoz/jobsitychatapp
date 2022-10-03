using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JobsityChatBot.Helpers;
using Microsoft.Extensions.Configuration;

namespace JobsityChatBot.Dialogs
{
    /// <summary>
    /// Principal dialog
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private static readonly HttpClient client = new();
        private readonly IConfiguration configuration;

        public MainDialog(StockDialog bookingDialog, ILogger<MainDialog> logger, IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            this.configuration = configuration;
            _logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Introduction for the user
        /// </summary>
        /// <param name="stepContext">Current steps of the Dialog</param>
        /// <param name="cancellationToken">Token for cancellation between threads</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"/stock=APPL.US\"";
            Activity promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        /// <summary>
        /// Step for ask stocks
        /// </summary>
        /// <param name="stepContext">Current steps of the Dialog</param>
        /// <param name="cancellationToken">Token for cancellation between threads</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string message = stepContext.Result.ToString();
            if (message.Trim().StartsWith("/stock="))
            {
                string connectionString = configuration.GetValue<string>("azureServiceQueue");
                string queueName = configuration.GetValue<string>("jobsityQueueName");
                JobsityChatBotHelper.ProcessMessage(connectionString, queueName, message);
            }
            
            return await stepContext.BeginDialogAsync(nameof(StockDialog), null, cancellationToken);
        }

        /// <summary>
        /// Step for give the stock value to the user
        /// </summary>
        /// <param name="stepContext">Current steps of the Dialog</param>
        /// <param name="cancellationToken">Token for cancellation between threads</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is Activity result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, result.Text, cancellationToken);
            }

            string promptMessage = "I will give you the Stock information in a few seconds";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}

