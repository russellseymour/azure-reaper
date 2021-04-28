
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// name of the storage account to connect to
param storageAccountName string

// key to use to connect to the storage account
param storageAccountKey string

// Variables ----------------------------------------------------------------

// determine the apiId to use and return
var apiId = concat('/subscriptions/', subscription().subscriptionId, '/providers/Microsoft.Web/locations/', location, '/managedApis/azurequeues')

// Resources ----------------------------------------------------------------

resource apiConnectionQueue 'Microsoft.Web/connections@2016-06-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    displayName: name
    api: {
      id: apiId
    }
    parameterValues: {
      storageAccount: storageAccountName
      sharedKey: storageAccountKey
    }
  }
}

output id string = apiConnectionQueue.id
output apiId string = apiId
