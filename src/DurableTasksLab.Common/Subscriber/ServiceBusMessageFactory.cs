using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using DurableTasksLab.Common.Messaging;
using System.Text.Json;

namespace DurableTasksLab.Common.Subscriber;

public static class ServiceBusMessageFactory
{
    const string clientSource = "DurableTasksLab.Client";
    const string listenerSource = "DurableTasksLab.Listener";
    const string contentType = "application/json";
    
    // For samples on using the CloudEvent schema refer to: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample11_CloudEvents.md
    
    public static ServiceBusMessage CreateSummaryMessage(string instanceId, string executionId, string? status = null)
    {
        var cloudEventPayload = new OrchestrationBatchMessage
        {
            InstanceId = instanceId,
            ExecutionId = executionId,
            Status = status
        };

        var cloudEvent = new CloudEvent(source: listenerSource, type: DurableTasksMessagingTypeConstants.SummaryMessage, cloudEventPayload);
        cloudEvent.Subject = $"{DurableTasksMessagingTypeConstants.SummaryMessage}-{instanceId}-{executionId}";
        cloudEvent.ExtensionAttributes.Add("contenttype", contentType);

        return CreateServiceBusMessage(instanceId, cloudEvent);
    }

    public static ServiceBusMessage CreateSimpleOrchestrationMessage(Guid invokeOperationId, int sequence)
    {
        var cloudEventPayload = new SimpleOrchestrationMessage
        {
            Id = invokeOperationId,
            Sequence = sequence,
            Message = "Sample Message"
        };

        var cloudEvent = new CloudEvent(source: listenerSource, type: DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage, cloudEventPayload);
        cloudEvent.Subject = $"{DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage}-{invokeOperationId}-{sequence}";
        cloudEvent.ExtensionAttributes.Add("contenttype", contentType);

        return CreateServiceBusMessage($"{invokeOperationId}-{sequence}", cloudEvent);
    }

    private static ServiceBusMessage CreateServiceBusMessage(string messageId, CloudEvent cloudEvent)
    {
        ServiceBusMessage message = new(JsonSerializer.Serialize(cloudEvent))
        {
            ContentType = contentType,
            MessageId = messageId,
            Subject = cloudEvent.Subject
        };
        return message;
    }
}

