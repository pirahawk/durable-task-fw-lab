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

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: 'logs${randomSuffix}'
  location: resourceGroup().location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai${randomSuffix}'
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
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

resource ContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource StorageBlobContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource StorageQueueContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
}

resource StorageTableContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
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

resource storageBlobContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(storageAccount.id, userPrincipalId, StorageBlobContributorRole.name)
  scope: storageAccount
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: StorageBlobContributorRole.id
    principalType: 'User'
    description: 'StorageBlobContributorRole'
  }
}

resource StorageQueueContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(storageAccount.id, userPrincipalId, StorageQueueContributorRole.name)
  scope: storageAccount
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: StorageQueueContributorRole.id
    principalType: 'User'
    description: 'StorageQueueContributorRole'
  }
}

resource storageTableContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' ={
  name: guid(storageAccount.id, userPrincipalId, StorageTableContributorRole.name)
  scope: storageAccount
  properties:{
    principalId: userPrincipalId
    roleDefinitionId: StorageTableContributorRole.id
    principalType: 'User'
    description: 'StorageTableContributorRole'
  }
}

