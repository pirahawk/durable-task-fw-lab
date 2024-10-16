using DurableTask.Core;
using DurableTasksLab.Common.Messaging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class SimpleOrchestration : TaskOrchestration<DTfxResult, SimpleOrchestrationMessage>
{
    public override async Task<DTfxResult> RunTask(OrchestrationContext context, SimpleOrchestrationMessage input)
    {
        var taskOneMessage = await context.ScheduleTask<string>(typeof(SimpleTaskOne), input);
        return new DTfxResult{
            Success = true,
            Message = taskOneMessage            
        };
    }
}

public class SimpleTaskOne : TaskActivity<SimpleOrchestrationMessage, string>
{
    protected override string Execute(TaskContext context, SimpleOrchestrationMessage input)
    {
        var returnMessage = $"Executing Task: {nameof(SimpleTaskOne)} - ID: {input.Id} Message: {input.Message}";
        Console.WriteLine(returnMessage);
        return returnMessage;
    }
}