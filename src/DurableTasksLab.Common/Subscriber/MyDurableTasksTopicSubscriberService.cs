using System.Formats.Asn1;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DurableTasksLab.Common.Subscriber;

public class MyDurableTasksTopicSubscriberService : BackgroundService
{
    private readonly IConfiguration configuration;
    private readonly IDurableTasksMessageHandler durableTasksMessageHandler;
    private ServiceBusClient? client;
    private ServiceBusProcessor? processor;

    public MyDurableTasksTopicSubscriberService(IConfiguration configuration, IDurableTasksMessageHandler durableTasksMessageHandler)
    {
        this.configuration = configuration;
        this.durableTasksMessageHandler = durableTasksMessageHandler;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sbNamespace = this.configuration["ServiceBus:Namespace"];
        var topicName = this.configuration["ServiceBus:Topic"];
        var subscriptionName = this.configuration["ServiceBus:Subscription"];

        this.client = new(sbNamespace, new DefaultAzureCredential());
        this.processor = this.client.CreateProcessor(topicName: topicName, subscriptionName: subscriptionName, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = 1
        });

        processor.ProcessMessageAsync += async (args) =>
        {
            await this.ProcessMessageAsync(args, stoppingToken);
        };
        processor.ProcessErrorAsync += async (args) =>
        {
            await this.ProcessErrorAsync(args, stoppingToken);
        };
        await processor.StartProcessingAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
    {

        var receivedMessage = args.Message;
        try
        {
            if (cancellationToken.IsCancellationRequested && this.processor != null)
            {
                await this.processor.StopProcessingAsync();
                await args.AbandonMessageAsync(receivedMessage);
                return;
            }

            if (receivedMessage?.Body != null)
            {
                // deserialize the message body into a CloudEvent
                CloudEvent? receivedCloudEvent = CloudEvent.Parse(receivedMessage.Body);
                if(receivedCloudEvent != null){
                    await this.durableTasksMessageHandler.HandleMessage(receivedCloudEvent);
                }
                //receivedCloudEvent.Data.ToObjectFromJson
            }

            await args.CompleteMessageAsync(receivedMessage);
        }
        catch (Exception e)
        {
            await args.DeadLetterMessageAsync(receivedMessage);
        }
    }

    private async Task ProcessErrorAsync(ProcessErrorEventArgs args, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested && this.processor != null)
        {
            await this.processor.StopProcessingAsync();
        }
        await Task.CompletedTask;
    }
}

