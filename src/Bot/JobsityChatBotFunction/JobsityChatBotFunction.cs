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

namespace JobsityChatBotFunction
{
    /// <summary>
    /// Main Azure function class
    /// </summary>
    public class JobsityChatBotFunction
    {
        /// <summary>
        /// Get the stock name from the user command
        /// </summary>
        /// <param name="message">User command</param>
        /// <returns>Stock name</returns>
        private static string GetStockName(string message)
        {
            string[] parts = message.Split("=");
            if (parts.Length >= 2)
            {
                foreach(string word in parts)
                {
                    if (Regex.Match(word, "\\w+[.]\\w+").Success)
                    {
                        return word;
                    }
                }
                throw new IncorrectSintaxException();
            }
            throw new IncorrectSintaxException();
        }

        /// <summary>
        /// Ask stooq.com for a stock value
        /// </summary>
        /// <param name="stock">Stock name</param>
        /// <returns></returns>
        /// <exception cref="StooqUnavailableException">Http exception</exception>
        private static async Task<MemoryStream> GetStockValues(string stock)
        {
            Dictionary<string, string> values = new();
            FormUrlEncodedContent content = new(values);
            try
            {
                using HttpResponseMessage response = await new HttpClient().PostAsync($"https://stooq.com/q/l/?s={stock}&f=sd2t2ohlcv&h&e=csv", content);
                byte[] responseContent = await response.Content.ReadAsByteArrayAsync();
                return new MemoryStream(responseContent);
            }
            catch (Exception)
            {
                throw new StooqUnavailableException();
            }
        }

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
        /// Calls the stooq.com API and parse the CSV to a Stock object
        /// </summary>
        /// <param name="stock">Stock name</param>
        /// <returns>Stock object</returns>
        private static Stock GetStockFromApi(string stock)
        {
            using StreamReader reader = new(GetStockValues(stock).Result);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            IEnumerable<Stock> records = csv.GetRecords<Stock>();
            return records.FirstOrDefault(p => p.Symbol.ToLower() == stock);
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
                stock = GetStockName(myQueueItem);
                Stock record = GetStockFromApi(stock);
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
