using Azure.Identity;
using DurableTask.AzureStorage;
using DurableTask.Core;
using Microsoft;
using Microsoft.Extensions.Configuration;

namespace DurableTasksLab.Common.DTfx;

public static class DurableTaskFactory
{
    public static async Task<AzureStorageOrchestrationService> CreateOrchestrationService(IConfiguration configuration)
    {
        var storageConnectionString = configuration["Storage:Connection"];
        var storageName = configuration["Storage:Name"];
        var taskHubName = configuration["DurableTasks:taskHubName"];

        Assumes.NotNullOrEmpty(storageName);
        Assumes.NotNullOrEmpty(storageConnectionString);
        Assumes.NotNullOrEmpty(taskHubName);

        var azCredential = new DefaultAzureCredential();

        var settings = new AzureStorageOrchestrationServiceSettings
        {
            StorageAccountClientProvider = new StorageAccountClientProvider(storageName, azCredential),
            TaskHubName = taskHubName,
        };
        var orchestrationServiceAndClient = new AzureStorageOrchestrationService(settings);
        await orchestrationServiceAndClient.CreateIfNotExistsAsync();
        return orchestrationServiceAndClient;
    }

    public static TaskHubClient CreateTaskHubClient(AzureStorageOrchestrationService storageOrchestrationService)
    {
        var taskHubClient = new TaskHubClient(storageOrchestrationService);
        return taskHubClient;
    }

    public static TaskHubWorker CreateTaskHubWorker(
        AzureStorageOrchestrationService storageOrchestrationService, 
        Core.HostBuildingNameVersionObjectManager<TaskOrchestration> orchestrationObjectManager,
        Core.HostBuildingNameVersionObjectManager<TaskActivity> activityObjectManager)
    {
        var taskHubWorker = new TaskHubWorker(
            storageOrchestrationService,
            orchestrationObjectManager,
            activityObjectManager
        );
        return taskHubWorker;
    }

    public static TaskHubWorker CreateTaskHubWorker(AzureStorageOrchestrationService storageOrchestrationService)
    {
        var taskHubWorker = new TaskHubWorker(
            storageOrchestrationService
        );
        return taskHubWorker;
    }
}
