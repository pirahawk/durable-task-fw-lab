using Azure.Messaging;

namespace DurableTasksLab.Common.Subscriber;

public interface IDurableTasksMessageHandler{
    Task HandleMessage(CloudEvent cloudEventMessage, Azure.Messaging.ServiceBus.ServiceBusSender sender);
}

