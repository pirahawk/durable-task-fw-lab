Param(
  [parameter(Mandatory=$true)]
  [string] $randomSuffix,
  [parameter(Mandatory=$true)]
  [string] $azGroupName
)

Write-Output "Executing in $PSScriptRoot"
Write-Output "Invoked with $randomSuffix $azGroupName $azDeploymentName $userAdId"

if($null -eq $(az group show -n $azGroupName)){
    $errorMessage = "Group $azGroupName does not exist, please create resource group and try again"
    throw $errorMessage
    exit 1
      
}else {
    Write-Output "Group $azGroupName exists. Deploying resources"
}

$azDeploymentName = "deployment-$azGroupName"
$dependenciesDeploymentName = "$azDeploymentName-dependencies"

Write-Output "Executing deployment $dependenciesDeploymentName"

$userAdId = $(az ad signed-in-user show --query "id" -o tsv)

az deployment group create -g $azGroupName -n $dependenciesDeploymentName -f $PSScriptRoot\bicep\azDependencies.bicep `
 --parameters `
 randomSuffix="$randomSuffix" `
 userPrincipalId="$userAdId"


 if($?){  # see https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_automatic_variables?view=powershell-7.4#section-1
    Write-Output "deployment $dependenciesDeploymentName success"
    $azSbNamespaceName = $(az servicebus namespace show -g $azGroupName -n "servicebusns$randomSuffix" --query "name" -o tsv)
    $azSbEndpoint = $(az servicebus namespace show -g $azGroupName -n "servicebusns$randomSuffix" --query "serviceBusEndpoint" -o tsv)
    $azSbTopic = $(az servicebus topic show -g $azGroupName -n durabletaskstopic --namespace-name "servicebusns$randomSuffix" --query "name" -o tsv)
    $azSbSubscription = $(az servicebus topic subscription show -g $azGroupName -n defaultmessagesub --topic-name durabletaskstopic --namespace-name "servicebusns$randomSuffix" --query "name" -o tsv)
    $azStorageEndpoint = $(az storage account show -g $azGroupName -n "dtstorage$randomSuffix" --query "primaryEndpoints.blob" -o tsv)
    $azStorageAccountName = $(az storage account show -g $azGroupName -n "dtstorage$randomSuffix" --query "name" -o tsv)
    $azStorageEndpoinConnectionString = $(az storage account show-connection-string -g $azGroupName -n "dtstorage$randomSuffix" --query "connectionString" -o tsv)
    $appInsightsConnectionString =$(az monitor app-insights component show --app "ai$randomSuffix" -g $azGroupName --query "connectionString" -o tsv)

    Write-Output "Created Az Service Bus Namespace: $azSbEndpoint"
    Write-Output "Created Az Service Bus Topic: $azSbTopic"
    Write-Output "Created Az Storage endpoint: $azStorageEndpoint"

    $DurableTasksLabServiceProjectPath = 'src/DurableTasksLab.Service/DurableTasksLab.Service.csproj'
    $DurableTasksLabListenerServiceProjectPath = 'src/DurableTasksLab.ListenerService/DurableTasksLab.ListenerService.csproj'
    $DurableTasksLabClientProjectPath = 'src/DurableTasksLab.Client/DurableTasksLab.Client.csproj'
    $(dotnet user-secrets init -p $DurableTasksLabServiceProjectPath)
    $(dotnet user-secrets init -p $DurableTasksLabListenerServiceProjectPath)
    $(dotnet user-secrets init -p $DurableTasksLabClientProjectPath)

    #Service Secrets
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'ServiceBus:Namespace' "$azSbNamespaceName.servicebus.windows.net")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'ServiceBus:Topic' "$azSbTopic")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'ServiceBus:Subscription' "$azSbSubscription")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'Storage:Connection' "$azStorageEndpoinConnectionString")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'Storage:Name' "$azStorageAccountName")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'DurableTasks:taskHubName' "MyDTFXHub")
    $(dotnet user-secrets -p $DurableTasksLabServiceProjectPath set 'ApplicationInsights:ConnectionString' "$appInsightsConnectionString")

    #Listener Secrets
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'ServiceBus:Namespace' "$azSbNamespaceName.servicebus.windows.net")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'ServiceBus:Topic' "$azSbTopic")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'ServiceBus:Subscription' "$azSbSubscription")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'Storage:Connection' "$azStorageEndpoinConnectionString")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'Storage:Name' "$azStorageAccountName")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'DurableTasks:taskHubName' "MyDTFXHub")
    $(dotnet user-secrets -p $DurableTasksLabListenerServiceProjectPath set 'ApplicationInsights:ConnectionString' "$appInsightsConnectionString")

    #Client Secrets
    $(dotnet user-secrets -p $DurableTasksLabClientProjectPath set 'ServiceBus:Namespace' "$azSbNamespaceName.servicebus.windows.net")
    $(dotnet user-secrets -p $DurableTasksLabClientProjectPath set 'ServiceBus:Topic' "$azSbTopic")
    $(dotnet user-secrets -p $DurableTasksLabClientProjectPath set 'ApplicationInsights:ConnectionString' "$appInsightsConnectionString")
 }