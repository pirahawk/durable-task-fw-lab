using DurableTask.Core;
using DurableTasksLab.Common.Messaging;

namespace DurableTasksLab.Common.DTfx.Orchestrations.Simple;

public class SimpleTaskOne : TaskActivity<SimpleOrchestrationMessage, string>
{
    protected override string Execute(TaskContext context, SimpleOrchestrationMessage input)
    {
        var returnMessage = $"Executing Task: {nameof(SimpleTaskOne)} - ID: {input.Id} Message: {input.Message}";
        Console.WriteLine(returnMessage);
        return returnMessage;
    }
}