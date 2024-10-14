Param(
  [parameter(Mandatory=$true)]
  [string] $randomizationGuid,
  [parameter(Mandatory=$true)]
  [string] $azGroupName
)

Write-Output "Executing in $PSScriptRoot"
Write-Output "Invoked with $randomizationGuid $azGroupName $azDeploymentName $userAdId"

$(dotnet user-secrets init -p 'src/DurableTasksLab.Service/DurableTasksLab.Service.csproj')
exit 0

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
 randomSuffix="$randomizationGuid" `
 userPrincipalId="$userAdId"


 if($?){  # see https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_automatic_variables?view=powershell-7.4#section-1
    Write-Output "deployment $dependenciesDeploymentName success"
    $azSbEndpoint = $(az servicebus namespace show -g $azGroupName -n "servicebusns$randomizationGuid" --query "serviceBusEndpoint" -o tsv)
    $azSbTopic = $(az servicebus topic show -g $azGroupName -n durabletaskstopic --namespace-name "servicebusns$randomizationGuid" --query "name" -o tsv)
    $azStorageEndpoint = $(az storage account show -g $azGroupName -n "dtstorage$randomizationGuid" --query "primaryEndpoints.blob" -o tsv)
    
    Write-Output "Created Az Service Bus Namespace: $azSbEndpoint"
    Write-Output "Created Az Service Bus Topic: $azSbTopic"
    Write-Output "Created Az Storage endpoint: $azStorageEndpoint"

    $(dotnet user-secrets init -p 'src/DurableTasksLab.Service/DurableTasksLab.Service.csproj')
 }