using DurableTask.Core;
using DurableTasksLab.Common.DTfx.Orchestrations.Simple;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DurableTasksLab.Common.Subscriber;

public class MyDurableTasksWorkerService : BackgroundService
{
    private readonly IConfiguration configuration;
    private readonly TaskHubWorker taskHubWorker;
    private readonly SimpleTaskOne simpleTaskOne;

    public MyDurableTasksWorkerService(
        IConfiguration configuration, 
        TaskHubWorker taskHubWorker, 
        SimpleTaskOne simpleTaskOne)
    {
        this.configuration = configuration;
        this.taskHubWorker = taskHubWorker;
        this.simpleTaskOne = simpleTaskOne;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Adapted from: https://github.com/Azure/durabletask/blob/a0ac4cc7b12b519649af2f4ec60960a9b6229630/samples/DurableTask.Samples/Program.cs#L164
        this.ConfigureWorkerOrchestrations(taskHubWorker);
        this.ConfigureWorkerActivites(taskHubWorker);

        await taskHubWorker.StartAsync();
    }

    private void ConfigureWorkerOrchestrations(TaskHubWorker taskHubWorker)
    {
        taskHubWorker.AddTaskOrchestrations(typeof(SimpleOrchestration));
    }

    private void ConfigureWorkerActivites(TaskHubWorker taskHubWorker)
    {
        taskHubWorker.AddTaskActivities(this.simpleTaskOne);
        
    }
}

