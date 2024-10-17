using DurableTask.Core;

namespace DurableTasksLab.Common.DTfx.Core;

public class OrchestrationObjectCreator<T> : HostBuildingObjectCreator<TaskOrchestration>, IHostBuildingObjectCreator where T : TaskOrchestration
{
    public OrchestrationObjectCreator(Func<T> creatorFactoryFunc) : base(creatorFactoryFunc) { }

    public override void Register(TaskHubWorker taskHubWorker)
    {
        taskHubWorker.AddTaskOrchestrations(this);
    }

    public override string Key => $"{typeof(T).FullName}_";
}
