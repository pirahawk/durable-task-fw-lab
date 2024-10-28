// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DurableTasksLab.Common.Subscriber;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
.Build();

var sbNamespace = configuration["ServiceBus:Namespace"];
var topicName = configuration["ServiceBus:Topic"];
await using ServiceBusClient client = new(sbNamespace, new DefaultAzureCredential());

ServiceBusSender sender = client.CreateSender(topicName);

var messageCollection = new List<ServiceBusMessage>();
var batchId = Guid.NewGuid();
var batchSize = 100;

for (int i = 0; i< batchSize; i++){
    await SendSimpleOrchestrationMessageAsync(batchId, i);
}

await sender.SendMessagesAsync(messageCollection);
//await SendSummaryMessageAsync(batchId, batchSize);

Console.WriteLine($"Sent messages to: {configuration["ServiceBus:Namespace"]} for batch - {batchId}");

async Task SendSimpleOrchestrationMessageAsync(Guid invokeOperationId, int sequence)
{
    var serviceBusMessage = ServiceBusMessageFactory.CreateSimpleOrchestrationMessage(invokeOperationId, sequence);
    messageCollection.Add(serviceBusMessage);
    await Task.CompletedTask;
    //await sender.SendMessageAsync(message);
}