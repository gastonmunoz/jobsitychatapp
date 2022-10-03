using Azure.Messaging.ServiceBus;
using System;

namespace JobsityChatBot.Helpers
{
    public static class JobsityChatBotHelper
    {
        private static readonly string connectionString = "Endpoint=sb://jobsityqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7tU4/Sb3bmrGdO3GTegjeO3hwpWg6sC/jKtPLUs7lps=";
        private static readonly string queueName = "jobsitychatqueue";

        public async static void ProcessMessage(string message)
        {
            string newMessage = message.Trim();
            ServiceBusClientOptions clientOptions = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient serviceBusClient = new(connectionString, clientOptions);
            ServiceBusSender sender = serviceBusClient.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(newMessage)))
            {
                throw new Exception($"The message {newMessage} is too large to fit in the batch.");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
            }
            finally
            {
                await sender.DisposeAsync();
                await serviceBusClient.DisposeAsync();
            }
        }
    }
}
