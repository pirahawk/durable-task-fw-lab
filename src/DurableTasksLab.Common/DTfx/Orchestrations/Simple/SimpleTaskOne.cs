using DurableTask.Core;
using DurableTasksLab.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class SimpleTaskOne : TaskActivity<SimpleOrchestrationMessage, string>
{
    private readonly ILogger<SimpleTaskOne>? logger;

    public SimpleTaskOne(ILogger<SimpleTaskOne> logger)
    {
        this.logger = logger;
    }

    protected override string Execute(TaskContext context, SimpleOrchestrationMessage input)
    {
        var returnMessage = $"Executing Task: {nameof(SimpleTaskOne)} - ID: {input.Id} Message: {input.Message}";
        this.logger?.LogInformation(returnMessage);
        return returnMessage;
    }
}