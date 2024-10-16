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


resource ServiceBusContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource ServiceBusSenderRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
}

resource ServiceBusReceiverRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
}

//b24988ac-6180-42a0-ab88-20f7382dd24c

resource ContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource sbContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(servicebusNameSpace.id, userPrincipalId, ServiceBusContributorRole.name)
  scope: servicebusNameSpace
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: ServiceBusContributorRole.id
    principalType: 'User'
    description: 'ServiceBusContributorRole'
  }
}

resource sbSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(servicebusNameSpace.id, userPrincipalId, ServiceBusSenderRole.name)
  scope: servicebusNameSpace
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: ServiceBusSenderRole.id
    principalType: 'User'
    description: 'ServiceBusSenderRole'
  }
}

resource sbReceiverAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(servicebusNameSpace.id, userPrincipalId, ServiceBusReceiverRole.name)
  scope: servicebusNameSpace
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: ServiceBusReceiverRole.id
    principalType: 'User'
    description: 'ServiceBusReceiverRole'
  }
}

resource storageContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(storageAccount.id, userPrincipalId, ContributorRole.name)
  scope: storageAccount
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: ContributorRole.id
    principalType: 'User'
    description: 'StorageContributorRole'
  }
}


