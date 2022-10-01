// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.16.0

using Azure.Messaging.ServiceBus;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityChatBot.Dialogs
{
    public class StockDialog : CancelAndHelpDialog
    {
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";

        public StockDialog()
            : base(nameof(StockDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());

            var waterfallSteps = new WaterfallStep[]
            {
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

        private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stock = (Stock)stepContext.Options;

            if (stock.Symbol == null)
            {
                var promptMessage = MessageFactory.Text(DestinationStepMsgText, DestinationStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(stock.Symbol, cancellationToken);
        }

        private async Task<DialogTurnResult> OriginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stock = (Stock)stepContext.Options;

            stock.Symbol = (string)stepContext.Result;

            if (stock.Symbol == null)
            {
                var promptMessage = MessageFactory.Text(OriginStepMsgText, OriginStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(stock.Symbol, cancellationToken);
        }

        private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stock = (Stock)stepContext.Options;

            stock.Symbol = (string)stepContext.Result;

            if (stock.Symbol == null || IsAmbiguous(stock.Symbol))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), stock.Symbol, cancellationToken);
            }

            return await stepContext.NextAsync(stock.Symbol, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stock = (Stock)stepContext.Options;

            stock.Symbol = (string)stepContext.Result;

            var messageText = $"Please confirm, I have you traveling to: {stock.Symbol} from: {stock.Symbol} on: {stock.Symbol}. Is this correct?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string connString = "Endpoint=sb://jobsityqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7tU4/Sb3bmrGdO3GTegjeO3hwpWg6sC/jKtPLUs7lps=";
            string queueName = "jobsitychatqueue";
            await using var client = new ServiceBusClient(connString);
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);
            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
                // add handler to process messages
                string body = receivedMessage.Body.ToString();
                var message = MessageFactory.Text(body, body, InputHints.IgnoringInput);
                return await stepContext.EndDialogAsync(message, cancellationToken);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await client.DisposeAsync();
            }
        }
    }
}
