using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Threading.Tasks;
using System.Linq;
using JobsityChatBotFunction.Classes;
using CsvHelper.TypeConversion;
using Azure.Messaging.ServiceBus;

namespace JobsityChatBotFunction
{
    public class JobsityChatBotFunction
    {
        private static readonly string connectionString = "Endpoint=sb://josbityresponsechat.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=meVqi4gMC7ATkmfSAKH90nCYZqf1sIA0Nu1pGZDG2ok=";
        private static readonly string queueName = "jobsityresponse";

        [FunctionName("jobsitychatbot")]
        public static async Task Run([ServiceBusTrigger("jobsitychatqueue", Connection = "jobsityqueue")]string myQueueItem, ILogger log)
        {
            string[] parts = myQueueItem.Split("=");
            string stock = "";
            if (parts.Length == 2)
            {
                stock = parts[1].ToLower();
            }
            Dictionary<string, string> values = new();
            FormUrlEncodedContent content = new(values);

            using HttpResponseMessage response = await new HttpClient().PostAsync($"https://stooq.com/q/l/?s={stock}&f=sd2t2ohlcv&h&e=csv", content);
            byte[] responseContent = await response.Content.ReadAsByteArrayAsync();
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            };
            MemoryStream stream = new(responseContent);
            //using StreamReader reader = new("C:\\Users\\Gaston\\Downloads\\aapl.us.csv");
            using StreamReader reader = new(stream);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            
            ServiceBusClientOptions clientOptions = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            ServiceBusClient serviceBusClient = new(connectionString, clientOptions);
            ServiceBusSender sender = serviceBusClient.CreateSender(queueName);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            try
            {
                IEnumerable<Stock> records = csv.GetRecords<Stock>();
                Stock record = records.FirstOrDefault(p => p.Symbol.ToLower() == stock);
                double average = (record.High + record.Low) / 2;


                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"{record.Symbol} quote is ${average} per share")))
                {
                    throw new Exception($"The message {record.Symbol} is too large to fit in the batch.");
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
            catch (TypeConverterException)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Stock \"{stock}\" unavailable in stooq.com.")))
                {
                    throw new Exception($"The message \"Stock \"{stock}\" unavailable in stooq.com.\" is too large to fit in the batch.");
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
            catch (Exception)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Cannot query for: \"{stock}\".")))
                {
                    throw new Exception($"The message \"Cannot query for: \"{stock}\".\" is too large to fit in the batch.");
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
}
