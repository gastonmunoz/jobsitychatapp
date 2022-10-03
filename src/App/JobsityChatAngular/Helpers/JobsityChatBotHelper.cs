using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JobsityChatAngular.Helpers
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
        public async static void ProcessMessage(IConfiguration configuration, string message, string groupName)
        {
            string connectionString = configuration.GetValue<string>("jobsityQueue");
            string queueName = configuration.GetValue<string>("jobsityQueueName");
            dynamic messageToPost = new
            {
                Message = message.Trim(),
                GroupName = groupName
            };
            ServiceBusClientOptions clientOptions = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient serviceBusClient = new(connectionString, clientOptions);
            ServiceBusSender sender = serviceBusClient.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
                
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(messageToPost))))
            {
                throw new Exception($"The message {messageToPost.message} is too large to fit in the batch.");
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
