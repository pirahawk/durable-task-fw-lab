# durable-task-fw-lab
A simple lab environment for experimenting with the [Azure Durable Task Framework (DTFx)](https://github.com/Azure/durabletask/tree/main).

## Build and Run

To build and run the lab, you will require a valid Azure Subscription to which you have rights to deploy and manage Azure resources. Please follow the steps below to setup and build the lab environment to allow experimenting with the `Azure Durable Task Framework`.

### System Requirements

The lab requires the following dependencies to be installed:

* [Dotnet 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.4)
* [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
* [Azure Bicep tools (via Azure CLI)](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#azure-cli)

### Deploy Azure Resources

The lab will build and run against deployed Azure Resources. Please begin by creating a target [Azure Resource Group](https://learn.microsoft.com/en-us/cli/azure/group?view=azure-cli-latest#az-group-create) like so:

```azurecli
az group create -l uksouth -n <target-azure-resource-group>
```

Once the resource group has been created, you will need to run the [deploy.ps1](./build/deploy.ps1) script. The script expects the following paramters:

* `azGroupName` - which should match the `<target-azure-resource-group>` parameter from the previous step.
* `randomSuffix` - a small random suffix of your choice, which will get appended to Azure resources created (in an effort to avoid any Azure Host Name collisions when deployed).


Run the [deploy.ps1](./build/deploy.ps1) script like so:

```powershell
$randomSuffix = "<your-random-suffix>"
$azGroupName = "<target-azure-resource-group>"

.$PSScriptRoot/build/deploy.ps1 `
-randomSuffix $randomSuffix `
-azGroupName $azGroupName
```

> Note: It is vital for the lifetime of the lab environment to keep the value of `<your-random-suffix>` constant, otherwise you risk re-deploying new resources every time the script is invoked.

The [deploy.ps1](./build/deploy.ps1) will make use of the [azDependencies.bicep](./build/bicep/azDependencies.bicep) script to deploy all required azure resources and setup RBAC permissions.

Once the deployment is successful, the script will automatically configure all application configuration using [the Dotnet Secret Manager tool](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows#secret-manager).

### Run the Solution

The [DurableTasksLab](./src/DurableTasksLab.sln) solution contains the following primary projects:

* [DurableTasksLab.Service](./src/DurableTasksLab.Service/DurableTasksLab.Service.csproj)
* [DurableTasksLab.Client](./src/DurableTasksLab.Client/DurableTasksLab.Client.csproj)
* [DurableTasksLab.Common](./src/DurableTasksLab.Common/DurableTasksLab.Common.csproj)

The premise behind the lab is that the `DurableTasksLab.Service` contains the hosting setup to be able to listen to messages sent to a target `Azure Service Bus Topic Subscription`, which will in turn invoke Durable Task Orchestrations as prescribed by the message (please see [IDurableTasksMessageHandler](./src/DurableTasksLab.Common/Subscriber/IDurableTasksMessageHandler.cs)).

The `DurableTasksLab.Client` is a simple console application that will send messages to a target `Azure Service Bus Topic`.

The lab comes bundled with a simple [Orchestration and Activity](./src/DurableTasksLab.Common/DTfx/Orchestrations/). Please feel free to extend and experiment with these as required. (I may add some more later - watch this space)

> Note: The orchestrations will need to be registered within the [MyDurableTasksWorkerService](./src/DurableTasksLab.Common/Subscriber/MyDurableTasksWorkerService.cs).

To Run the solution locally:

1. Run the [DurableTasksLab.Service](./src/DurableTasksLab.Service/DurableTasksLab.Service.csproj) to bring up the hosting for the durable tasks.
1. Invoke the [DurableTasksLab.Client](./src/DurableTasksLab.Client/DurableTasksLab.Client.csproj) - This will send a message to the target `Azure Service Bus Topic` which will in turn invoke the target orchestration on the Service Host. 

## Useful links
* https://github.com/Azure/durabletask/wiki/Core-Concepts
* https://github.com/Azure/durabletask
* https://learn.microsoft.com/en-us/shows/on-dotnet/building-workflows-with-the-durable-task-framework
* https://abhikmitra.github.io/blog/durable-task/
* https://www.primesoft.net/durable-task-framework-for-long-running-workflows
* https://github.com/Azure/durabletask/blob/main/samples/DurableTask.Samples/Greetings/SendGreetingTask.cs