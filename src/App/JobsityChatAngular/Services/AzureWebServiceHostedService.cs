using Azure.Messaging.ServiceBus;
using Duende.IdentityServer.Models;
using JobsityChatAngular.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using static JobsityChatAngular.Hubs.JobsityChatHub;

namespace JobsityChatAngular.Services
{
    /// <summary>
    /// HostedService for Azure Service Bus
    /// </summary>
    public class AzureWebServiceHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<AzureWebServiceHostedService> _logger;
        private readonly IHubContext<JobsityChatHub> hubContext;
        private readonly IConfiguration configuration;
        static ServiceBusClient client;
        static ServiceBusProcessor processor;

        public AzureWebServiceHostedService(ILogger<AzureWebServiceHostedService> logger,
            IHubContext<JobsityChatHub> hubContext, IConfiguration configuration)
        {
            _logger = logger;
            this.hubContext = hubContext;
            this.configuration = configuration;
        }

        /// <summary>
        /// Starts the Hosted Service
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken stoppingToken)
        {
            string connectionString = configuration.GetValue<string>("jobsityResponse");
            string queueName = configuration.GetValue<string>("jobsityResponseName");
            ServiceBusClientOptions clientOptions = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(connectionString, clientOptions);
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync(stoppingToken);
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            JObject obj = JObject.Parse(body);
            string message = obj.Property("Message").Value.ToString();
            string groupName = obj.Property("GroupName").Value.ToString();
            NewMessage newMessage = new(DateTime.Now, "Jobsity Stock Assistant", message, groupName);
            await hubContext.Clients.Group(groupName).SendAsync("NewMessage", newMessage);
        }

        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop the hosted service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                await processor.StopProcessingAsync(stoppingToken);
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        /// <summary>
        /// Final disposes
        /// </summary>
        public void Dispose()
        {
            Console.Write("dispose");
        }
    }
}
