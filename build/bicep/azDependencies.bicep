param randomSuffix string
param userPrincipalId string


resource servicebusNameSpace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'servicebusns${randomSuffix}'
  location: resourceGroup().location
  sku:{
    name:'Standard'
    tier: 'Standard'
  }
  identity:{
    type:'SystemAssigned'
  }
  properties:{
    zoneRedundant: false
  }

  resource messagepubtopic 'topics@2022-10-01-preview' = {
    name: 'durabletaskstopic'
    properties:{
      requiresDuplicateDetection:false
      defaultMessageTimeToLive: 'PT10M'
    }

    resource subscription 'subscriptions' = {
      name: 'defaultmessagesub'
      properties: {
        deadLetteringOnFilterEvaluationExceptions: true
        deadLetteringOnMessageExpiration: true
        maxDeliveryCount: 10
      }
    }

  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'dtstorage${toLower(randomSuffix)}'
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}
