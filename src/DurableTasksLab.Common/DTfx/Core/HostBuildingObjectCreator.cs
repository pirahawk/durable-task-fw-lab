using DurableTask.Core;

namespace DurableTasksLab.Common.DTfx.Core;

public abstract class HostBuildingObjectCreator<T> : ObjectCreator<T>, IHostBuildingObjectCreator where T : class
{
    private readonly Func<T> creatorFactoryFunc;

    public HostBuildingObjectCreator(Func<T> creatorFactoryFunc)
    {
        this.creatorFactoryFunc = creatorFactoryFunc;
    }

    public abstract string Key { get; }

    public override T Create()
    {
        return creatorFactoryFunc();
    }

    public abstract void Register(TaskHubWorker taskHubWorker);
}
