// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using DurableTasksLab.Common.Messaging;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
.Build();

var sbNamespace = configuration["ServiceBus:Namespace"];
var topicName = configuration["ServiceBus:Topic"];
await using ServiceBusClient client = new(sbNamespace, new DefaultAzureCredential());

ServiceBusSender sender = client.CreateSender(topicName);

// For samples on using the CloudEvent schema refer to: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample11_CloudEvents.md
var source = "DurableTasksLab.Client";

var contentType = "application/json";

for (int i = 0; i< 100; i++){
    var invokeOperationId = Guid.NewGuid();
    await SendSimpleOrchestrationMessageAsync(invokeOperationId);
}

Console.WriteLine($"Sent messages to: {configuration["ServiceBus:Namespace"]}");

async Task SendSimpleOrchestrationMessageAsync(Guid invokeOperationId)
{
    var cloudEventPayload = new SimpleOrchestrationMessage
    {
        Id = invokeOperationId,
        Message = "Sample Message"
    };

    var cloudEvent = new CloudEvent(source: source, type: DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage, cloudEventPayload);
    cloudEvent.Subject = $"{DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage}-{invokeOperationId}";
    cloudEvent.ExtensionAttributes.Add("contenttype", contentType);

    await SendServiceBusTopicMessageAsync(invokeOperationId, cloudEvent);
}

async Task SendServiceBusTopicMessageAsync(Guid invokeOperationId, CloudEvent cloudEvent)
{
    // create a message that we can send. UTF-8 encoding is used when providing a string.
    ServiceBusMessage message = new(JsonSerializer.Serialize(cloudEvent))
    {
        ContentType = contentType,
        MessageId = $"{invokeOperationId}-{Guid.NewGuid()}",
        Subject = cloudEvent.Subject
    };

    // send the message
    await sender.SendMessageAsync(message);
}