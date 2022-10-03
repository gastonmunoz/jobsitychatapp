using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using System.Threading.Tasks;
using System.Linq;
using JobsityChatBotFunction.Classes;
using CsvHelper.TypeConversion;
using Azure.Messaging.ServiceBus;
using JobsityChatBotFunction.Exceptions;
using System.Text.RegularExpressions;
using System.Collections;
using JobsityChatBotFunction.Helpers;

namespace JobsityChatBotFunction
{
    /// <summary>
    /// Main Azure function class
    /// </summary>
    public class JobsityChatBotFunction
    {

        /// <summary>
        /// Post the bot response (stock value) to an Azure Service Bus queue
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <exception cref="Exception"></exception>
        private static async void PostMessageToServiceBus(string message)
        {
            string connString = Environment.GetEnvironmentVariable("responseQueue");
            string queueName = Environment.GetEnvironmentVariable("responseQueueName");

            ServiceBusClientOptions clientOptions = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient serviceBusClient = new(connString, clientOptions);
            ServiceBusSender sender = serviceBusClient.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(message)))
            {
                throw new Exception($"The message {message} is too large to fit in the batch.");
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

        /// <summary>
        /// Azure function method
        /// </summary>
        /// <param name="myQueueItem">Value received from the Azure ServiceBus trigger</param>
        /// <param name="log">Logger</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [FunctionName("jobsitychatbot")]
        public static async Task Run([ServiceBusTrigger("jobsitychatqueue", Connection = "jobsityqueue")]string myQueueItem, ILogger log)
        {
            string stock = "";
            try
            {
                stock = JobsityChatBotHelper.GetStockName(myQueueItem);
                Stock record = JobsityChatBotHelper.GetStockFromApi(stock, new HttpClient());
                double average = (record.High + record.Low) / 2;
                PostMessageToServiceBus($"{record.Symbol} quote is ${average} per share");
            }
            catch (TypeConverterException)
            {
                PostMessageToServiceBus($"Stock \"{stock}\" unavailable in stooq.com.");
            }
            catch (Exception e)
            {
                if (e is StooqUnavailableException || e is StooqUnavailableException)
                {
                    PostMessageToServiceBus(e.Message);
                }
                else
                {
                    PostMessageToServiceBus($"Cannot query for: \"{stock}\".");
                }
            }
        }
    }
}
