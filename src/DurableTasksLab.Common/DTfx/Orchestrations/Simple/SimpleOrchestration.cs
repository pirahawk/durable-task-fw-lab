using DurableTask.Core;
using DurableTasksLab.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class SimpleOrchestration : TaskOrchestration<DTfxResult, SimpleOrchestrationMessage>
{
    private readonly ILogger<SimpleOrchestration>? logger;

    public SimpleOrchestration():base(){}

    public SimpleOrchestration(ILogger<SimpleOrchestration> logger):this()
    {
        this.logger = logger;
    }

    public override async Task<DTfxResult> RunTask(OrchestrationContext context, SimpleOrchestrationMessage input)
    {
        var returnMessage = $"Executing Orchestration: {nameof(SimpleOrchestration)} - ID: {input.Id} Message: {input.Message}";
        this.logger?.LogInformation(returnMessage);
        var taskOneMessage = await context.ScheduleTask<string>(typeof(SimpleTaskOne), input);
        return new DTfxResult{
            Success = true,
            Message = taskOneMessage            
        };
    }
}
