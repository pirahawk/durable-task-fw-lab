using DurableTask.Core;
using DurableTasksLab.Common.Messaging;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class TrackPerformanceTask : TaskActivity<OrchestrationTelemetryInformation, string>
{
    private TelemetryClient telemetryClient;
    private readonly ILogger<SimpleTaskOne>? logger;

    public TrackPerformanceTask(ILogger<SimpleTaskOne> logger, TelemetryClient telemetryClient)
    {
        this.logger = logger;
        this.telemetryClient = telemetryClient;
    }

    protected override string Execute(TaskContext context, OrchestrationTelemetryInformation input)
    {
        var orchestrationInstance = context.OrchestrationInstance.InstanceId;
        var unixTime = ((DateTimeOffset)input.OrchestrationStartTimeUTC).ToUnixTimeSeconds();
        this.telemetryClient.TrackMetric($"orchestration-{orchestrationInstance}", 1, new Dictionary<string,string>{
            {"OrchestrationInstanceId", orchestrationInstance},
            {"OrchestrationExecutionId", context.OrchestrationInstance.ExecutionId},
            {"OrchestrationStartTime", $"{input.OrchestrationStartTimeUTC}"},
            {"OrchestrationStartTimeTicks", $"{unixTime}"}
        });
        var returnMessage = $"Executing Task: {nameof(SimpleTaskOne)} - ID: {context.OrchestrationInstance.InstanceId} ExecutionId: {context.OrchestrationInstance.ExecutionId} OrchestrationStartTime:{input.OrchestrationStartTimeUTC}";
        this.logger?.LogInformation(returnMessage);
        return returnMessage;
    }
}