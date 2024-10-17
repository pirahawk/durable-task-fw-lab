using DurableTask.Core;
using DurableTasksLab.Common.DTfx.Core;
using Microsoft;

public static class HostBuilderExtensions
{
    public static void AddHostBuildingNameVersionObjectManager<T>(this IServiceCollection serviceCollection) where T:class
    {
        serviceCollection.AddTransient<HostBuildingNameVersionObjectManager<T>>((serviceProvider) =>
        {
            var logger = serviceProvider.GetService<ILogger<HostBuildingNameVersionObjectManager<T>>>();
            Assumes.NotNull(logger);
            return new HostBuildingNameVersionObjectManager<T>(serviceProvider, logger);
        });
    }

    public static void AddOrchestration<T>(this IServiceCollection serviceCollection) where T : TaskOrchestration
    {
        serviceCollection.AddTransient<T>();
        serviceCollection.AddTransient<IHostBuildingObjectCreator, OrchestrationObjectCreator<T>>((serviceProvider) => {
            return new OrchestrationObjectCreator<T>(() => serviceProvider.GetService<T>()!);
        });
    }

    public static void AddActivity<T>(this IServiceCollection serviceCollection) where T : TaskActivity
    {
        serviceCollection.AddTransient<T>();
        serviceCollection.AddTransient<IHostBuildingObjectCreator, ActivityObjectCreator<T>>((serviceProvider) => {
            return new ActivityObjectCreator<T>(() => serviceProvider.GetService<T>()!);
        });
    }
}