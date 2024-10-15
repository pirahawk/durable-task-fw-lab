using Azure.Messaging;
using DurableTasksLab.Common.Messaging;
using Microsoft;

namespace DurableTasksLab.Common.Subscriber;

public class MyDurableTasksMessageHandler : IDurableTasksMessageHandler
{
    public async Task HandleMessage(CloudEvent cloudEventMessage)
    {
        var eventtype = cloudEventMessage.Type;

        switch (eventtype)
        {
            case DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage:
                await HandleSimpleOrchestrationMessage(cloudEventMessage);
                return;
        }
    }

    private async Task HandleSimpleOrchestrationMessage(CloudEvent cloudEventMessage)
    {
        Assumes.NotNull(cloudEventMessage);
        Assumes.NotNull(cloudEventMessage.Data);

        var messageData = cloudEventMessage.Data.ToObjectFromJson<SimpleOrchestrationMessage>();
    }
}

