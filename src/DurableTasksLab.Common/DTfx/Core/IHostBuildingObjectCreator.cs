using DurableTask.Core;

namespace DurableTasksLab.Common.DTfx.Core;

public interface IHostBuildingObjectCreator
{
    string Key { get; }

    void Register(TaskHubWorker taskHubWorker);
}
