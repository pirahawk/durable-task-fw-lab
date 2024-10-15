// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
.Build();

var sbNamespace = configuration["ServiceBus:Namespace"];
var topicName = configuration["ServiceBus:Topic"];
await using ServiceBusClient client = new(sbNamespace, new DefaultAzureCredential());

ServiceBusSender sender = client.CreateSender(topicName);

var invokeOperationId = Guid.NewGuid();
var contentType = "application/json";

// For samples on using the CloudEvent schema refer to: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample11_CloudEvents.md

dynamic cloudEventPayload = new {
    Id = invokeOperationId,
    Message = "Sample Message"
};
var cloudEvent = new CloudEvent("durabletask-client", "test-durabletask-invoke", cloudEventPayload);
cloudEvent.Subject = $"operation-{invokeOperationId}";

cloudEvent.ExtensionAttributes.Add("contenttype", contentType);

// create a message that we can send. UTF-8 encoding is used when providing a string.
ServiceBusMessage message = new(JsonSerializer.Serialize(cloudEvent)){
    ContentType = contentType,
    MessageId = $"{invokeOperationId}",
    Subject = cloudEvent.Subject
};

// send the message
await sender.SendMessageAsync(message);

Console.WriteLine($"Sent messages to: {configuration["ServiceBus:Namespace"]}");



