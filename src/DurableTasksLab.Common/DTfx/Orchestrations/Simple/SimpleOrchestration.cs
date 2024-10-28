using DurableTask.Core;
using DurableTasksLab.Common.Messaging;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class SimpleOrchestration : TaskOrchestration<DTfxResult, SimpleOrchestrationMessage>
{
    private readonly ILogger<SimpleOrchestration>? logger;
    private TelemetryClient telemetryClient;

    public SimpleOrchestration(ILogger<SimpleOrchestration> logger, TelemetryClient telemetryClient)
    {
        this.logger = logger;
        this.telemetryClient = telemetryClient;
    }

    public override async Task<DTfxResult> RunTask(OrchestrationContext context, SimpleOrchestrationMessage input)
    {
        //https://learn.microsoft.com/en-us/azure/azure-monitor/app/get-metric
        //this.telemetryClient.GetMetric("SimpleOrchestration").TrackValue(1);
        
        // this.telemetryClient.TrackMetric("SimpleOrchestration-Start", 1);

        var currentTime = context.CurrentUtcDateTime;

        var startMessage = $"Executing Orchestration: {nameof(SimpleOrchestration)} - ID: {input.Id} Message: {input.Message} IsReplaying: {context.IsReplaying}";
        this.logger?.LogInformation(startMessage);
        
        //var taskResponseMessage = await context.ScheduleTask<string>(typeof(SimpleTaskOne), input);

        var taskInput = new OrchestrationTelemetryInformation{
            OrchestrationInstanceId = context.OrchestrationInstance.InstanceId,
            OrchestrationStartTimeUTC = currentTime,
        };
        var taskResponseMessage = await context.ScheduleTask<string>(typeof(TrackPerformanceTask), taskInput);

        var returnMessage = $"Finished Executing Orchestration: {nameof(SimpleOrchestration)} - ID: {input.Id} Message: {input.Message} IsReplaying: {context.IsReplaying}";
        this.logger?.LogInformation(returnMessage);

        return new DTfxResult{
            Success = true,
            Message = taskResponseMessage            
        };
    }
}
