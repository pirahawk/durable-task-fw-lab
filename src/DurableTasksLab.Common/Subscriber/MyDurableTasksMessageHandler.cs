using Azure.Messaging;
using DurableTask.Core;
using DurableTasksLab.Common.DTfx.Orchestrations.Simple;
using DurableTasksLab.Common.Messaging;
using Microsoft;

namespace DurableTasksLab.Common.Subscriber;

public class MyDurableTasksMessageHandler : IDurableTasksMessageHandler
{
    private readonly TaskHubClient taskHubClient;

    public MyDurableTasksMessageHandler(TaskHubClient taskHubClient)
    {
        this.taskHubClient = taskHubClient;
    }
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
        Assumes.NotNull(messageData);

        OrchestrationInstance orchestrationInstance = await this.taskHubClient.CreateOrchestrationInstanceAsync(
            typeof(SimpleOrchestration), 
            messageData.Id.ToString(), 
            messageData);

         Console.WriteLine("Workflow Instance Started: " + orchestrationInstance);
    }
}

