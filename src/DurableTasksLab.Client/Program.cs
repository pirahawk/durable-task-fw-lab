// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
.Build();

var sbNamespace = configuration["ServiceBus:Namespace"];
var topicName = configuration["ServiceBus:Topic"];
await using ServiceBusClient client = new(sbNamespace, new DefaultAzureCredential());

ServiceBusSender sender = client.CreateSender(topicName);

// create a message that we can send. UTF-8 encoding is used when providing a string.
ServiceBusMessage message = new("Hello world!");

// send the message
await sender.SendMessageAsync(message);

Console.WriteLine($"Hello, World! {configuration["ServiceBus:Namespace"]}");



