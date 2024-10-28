using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using DurableTask.Core;
using DurableTasksLab.Common.DTfx.Orchestrations.Simple;
using DurableTasksLab.Common.Messaging;
using Microsoft;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace DurableTasksLab.Common.Subscriber;

public class MyDurableTasksMessageHandler : IDurableTasksMessageHandler
{
    private readonly TaskHubClient taskHubClient;
    private readonly ILogger<MyDurableTasksMessageHandler> logger;
    private TelemetryClient telemetryClient;

    public MyDurableTasksMessageHandler(TaskHubClient taskHubClient, ILogger<MyDurableTasksMessageHandler> logger, TelemetryClient telemetryClient)
    {
        this.taskHubClient = taskHubClient;
        this.logger = logger;
        this.telemetryClient = telemetryClient;
    }
    public async Task HandleMessage(CloudEvent cloudEventMessage, ServiceBusSender sender)
    {
        var eventtype = cloudEventMessage.Type;

        switch (eventtype)
        {
            case DurableTasksMessagingTypeConstants.SimpleOrchestrationMessage:
                await HandleSimpleOrchestrationMessage(cloudEventMessage, sender);
                return;
            case DurableTasksMessagingTypeConstants.SummaryMessage:
                await HandleSummary(cloudEventMessage, sender);
                return;
        }
    }

    private async Task HandleSummary(CloudEvent cloudEventMessage, ServiceBusSender sender)
    {
        Assumes.NotNull(cloudEventMessage);
        Assumes.NotNull(cloudEventMessage.Data);

        var messageData = cloudEventMessage.Data.ToObjectFromJson<OrchestrationBatchMessage>();
        Assumes.NotNull(messageData);
        var orchestrationInstance = new OrchestrationInstance
        {
            ExecutionId = messageData.ExecutionId,
            InstanceId = messageData.InstanceId,
        };

        var currentstate = await this.taskHubClient.GetOrchestrationStateAsync(orchestrationInstance);
        if (currentstate.OrchestrationStatus == OrchestrationStatus.Running || currentstate.OrchestrationStatus == OrchestrationStatus.Pending)
        {
            var serviceBusMessage = ServiceBusMessageFactory.CreateSummaryMessage(
                orchestrationInstance.InstanceId!, 
                orchestrationInstance.ExecutionId!, 
                currentstate.Status);

            await sender.SendMessageAsync(serviceBusMessage);
            return;
        }

        this.telemetryClient.TrackMetric($"orchestrationsummary-{currentstate.OrchestrationInstance.InstanceId}", 1, new Dictionary<string, string>{
            {"OrchestrationExecutionId", currentstate.OrchestrationInstance.ExecutionId},
            {"OrchestrationInstanceId", currentstate.OrchestrationInstance.InstanceId},
            {"OrchestrationState", $"{currentstate.OrchestrationStatus.ToString()}"},
            {"OrchestrationCreatedTime", $"{((DateTimeOffset)currentstate.CreatedTime).ToUnixTimeSeconds()}"},
            {"OrchestrationCompletedTime", $"{((DateTimeOffset)currentstate.CompletedTime).ToUnixTimeSeconds()}"}
        });
    }

    private async Task HandleSimpleOrchestrationMessage(CloudEvent cloudEventMessage, ServiceBusSender sender)
    {
        Assumes.NotNull(cloudEventMessage);
        Assumes.NotNull(cloudEventMessage.Data);

        var messageData = cloudEventMessage.Data.ToObjectFromJson<SimpleOrchestrationMessage>();
        Assumes.NotNull(messageData);

        OrchestrationInstance orchestrationInstance = await this.taskHubClient.CreateOrchestrationInstanceAsync(
            typeof(SimpleOrchestration),
            $"{messageData.Id}-{messageData.Sequence}",//messageData.Id.ToString(),
            messageData);

        this.logger.LogInformation($"Workflow Instance Started: {orchestrationInstance}");

        var serviceBusMessage = ServiceBusMessageFactory.CreateSummaryMessage(orchestrationInstance.InstanceId, orchestrationInstance.ExecutionId);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}

