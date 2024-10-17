using DurableTask.Core;
using Microsoft;

namespace DurableTasksLab.Common.DTfx.Core;

public class HostBuildingObjectCreator<T> : ObjectCreator<T> where T : class
{
    private readonly IServiceProvider serviceProvider;
    private readonly ObjectCreator<T> targetObjectCreator;

    public HostBuildingObjectCreator(IServiceProvider serviceProvider, ObjectCreator<T> targetObjectCreator)
    {
        this.serviceProvider = serviceProvider;
        this.targetObjectCreator = targetObjectCreator;
    }

    public override T Create()
    {
        var target = targetObjectCreator.Create();
        Assumes.NotNull(target);
        var resolvedTarget = this.serviceProvider.GetService(target.GetType()) as T;
        return resolvedTarget ?? throw new NullReferenceException(message: $"HostBuildingObjectCreator Could Not Instantiate object of type: {typeof(T).FullName}");
    }
}
