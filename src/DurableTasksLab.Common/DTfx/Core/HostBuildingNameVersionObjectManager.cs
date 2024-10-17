using DurableTask.Core;
using Microsoft;
using Microsoft.Extensions.Logging;

namespace DurableTasksLab.Common.DTfx.Core;

public class HostBuildingNameVersionObjectManager<T> : INameVersionObjectManager<T> where T : class
{
    readonly IDictionary<string, HostBuildingObjectCreator<T>> creators;
    readonly object thisLock = new object();
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<HostBuildingNameVersionObjectManager<T>> logger;

    public HostBuildingNameVersionObjectManager(IServiceProvider serviceProvider, ILogger<HostBuildingNameVersionObjectManager<T>> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        creators = new Dictionary<string, HostBuildingObjectCreator<T>>();
    }

    public void Add(ObjectCreator<T> creator)
    {
        lock (this.thisLock)
        {
            string key = GetKey(creator.Name, creator.Version);

            if (this.creators.ContainsKey(key))
            {
                throw new InvalidOperationException("Duplicate entry detected: " + creator.Name + " " +
                                                    creator.Version);
            }

            var hostObjectCreator = new HostBuildingObjectCreator<T>(this.serviceProvider, creator);
            this.creators.Add(key, hostObjectCreator);
        }
    }

    public T? GetObject(string name, string? version)
    {
        string key = GetKey(name, version);

            lock (this.thisLock)
            {
                if (this.creators.TryGetValue(key, out HostBuildingObjectCreator<T>? creator))
                {
                    Assumes.NotNull(creator);
                    return creator.Create();
                }

                return default(T);
            }
    }

    string GetKey(string name, string version)
    {
        return name + "_" + version;
    }
}
