using DurableTask.Core;

namespace DurableTasksLab.Common.DTfx.Core;

public class ActivityObjectCreator<T> : HostBuildingObjectCreator<TaskActivity> where T : TaskActivity
{
    public ActivityObjectCreator(Func<T> creatorFactoryFunc) : base(creatorFactoryFunc) { }

    public override void Register(TaskHubWorker taskHubWorker)
    {
        taskHubWorker.AddTaskActivities(this);
    }

    public override string Key => $"{typeof(T).FullName}_";
}
