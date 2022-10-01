// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.16.0

using Azure.Messaging.ServiceBus;
using JobsityChatBot.CognitiveModels;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        //private readonly FlightBookingRecognizer _luisRecognizer;
        private readonly ILogger _logger;
        private static readonly HttpClient client = new HttpClient();

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(/*FlightBookingRecognizer luisRecognizer, */StockDialog bookingDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            //_luisRecognizer = luisRecognizer;
            _logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);

            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
        {
            var unsupportedCities = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
            {
                unsupportedCities.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
            {
                unsupportedCities.Add(toEntities.To);
            }

            if (unsupportedCities.Any())
            {
                var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_luisRecognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

            //    return await stepContext.NextAsync(null, cancellationToken);
            //}

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"/stock=APPL.US\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string message = stepContext.Result.ToString();
            string[] parts = message.Split("=");
            string stock = "";
            if (parts.Length == 2)
            {
                stock = parts[1].ToLower();
            }
            Dictionary<string, string> values = new();
            FormUrlEncodedContent content = new(values);

            using HttpResponseMessage response = await client.PostAsync($"https://stooq.com/q/l/?s={stock}&f=sd2t2ohlcv&h&e=csv", content, cancellationToken);
            byte[] responseContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            };

            var stream = new MemoryStream(responseContent);
            using StreamReader reader = new("C:\\Users\\Gaston\\Downloads\\aapl.us.csv");
            //using StreamReader reader = new(stream);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            IEnumerable<Stock> records = csv.GetRecords<Stock>();

            //PostMessageQueue(records.FirstOrDefault());
            Stock record = records.FirstOrDefault();

            string connString = "Endpoint=sb://jobsityqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7tU4/Sb3bmrGdO3GTegjeO3hwpWg6sC/jKtPLUs7lps=";
            string queueName = "jobsitychatqueue";
            ServiceBusSender sender;
            int numOfMessages = 1;
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient serviceBusClient = new(connString, clientOptions);
            sender = serviceBusClient.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            //Stock record = records.FirstOrDefault();
            double average = (record.High + record.Low) / 2;
            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"{record.Symbol} quote is ${average} per share")))
            {
                throw new Exception($"The message {record.Symbol} is too large to fit in the batch.");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                return await stepContext.BeginDialogAsync(nameof(StockDialog), record, cancellationToken);
            }
            finally
            {
                await sender.DisposeAsync();
                await serviceBusClient.DisposeAsync();
            }


            //if (!_luisRecognizer.IsConfigured)
            //{
            //    // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
            //    return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            //}

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            //var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            //switch (luisResult.TopIntent().intent)
            //{
            //    case FlightBooking.Intent.BookFlight:
            //        await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

            //        // Initialize BookingDetails with any entities we may have found in the response.
            //        var bookingDetails = new BookingDetails()
            //        {
            //            // Get destination and origin from the composite entities arrays.
            //            Destination = luisResult.ToEntities.Airport,
            //            Origin = luisResult.FromEntities.Airport,
            //            TravelDate = luisResult.TravelDate,
            //        };

            //        // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //        return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

            //    case FlightBooking.Intent.GetWeather:
            //        // We haven't implemented the GetWeatherDialog so we just display a TODO message.
            //        var getWeatherMessageText = "TODO: get weather flow here";
            //        var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
            //        await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
            //        break;

            //    default:
            //        // Catch all for unhandled intents
            //        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
            //        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            //        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            //        break;
            //}

            //return await stepContext.NextAsync(null, cancellationToken);
        }

        private async void PostMessageQueue(Stock record)
        {
            string connString = "Endpoint=sb://jobsityqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7tU4/Sb3bmrGdO3GTegjeO3hwpWg6sC/jKtPLUs7lps=";
            string queueName = "jobsitychatqueue";
            ServiceBusSender sender;
            int numOfMessages = 1;
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient client = new(connString, clientOptions);
            sender = client.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            //Stock record = records.FirstOrDefault();
            double average = (record.High + record.Low) / 2;
            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"{record.Symbol} quote is ${average} per share")))
            {
                throw new Exception($"The message {record.Symbol} is too large to fit in the batch.");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }



        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is Activity result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, result.Text, cancellationToken);
                //var timeProperty = new TimexProperty(result.symbo);
                //var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                //var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                //var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                //await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "I will give you the Stock information in a few seconds";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}

public class Stock
{
    [Name("Symbol")]
    public string Symbol { get; set; }
    [Name("Date")]
    public string Date { get; set; }
    [Name("Time")]
    public string Time { get; set; }
    [Name("Open")]
    public double Open { get; set; }
    [Name("High")]
    public double High { get; set; }
    [Name("Low")]
    public double Low { get; set; }
    [Name("Close")]
    public double Close { get; set; }
    [Name("Volume")]
    public long Volume { get; set; }
}
