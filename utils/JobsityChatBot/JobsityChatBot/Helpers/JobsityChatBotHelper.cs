using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;

namespace JobsityChatBot.Helpers
{
    /// <summary>
    /// Helper for JobsityChatBot
    /// </summary>
    public static class JobsityChatBotHelper
    {
        /// <summary>
        /// Send messages to the Azure Service Bus queue
        /// </summary>
        /// <param name="configuration">App configuration</param>
        /// <param name="message">Message for the bot queue</param>
        /// <exception cref="Exception"></exception>
        public async static void ProcessMessage(string connectionString, string queueName, string message)
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
