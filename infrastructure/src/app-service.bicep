
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// id of the serviceplan
param servicePlanId string

// Conection string for the storage account
param storageAccountConnectionString string

// the instrumentation key for application insights
param applicationInsightsInstrumenationKey string

// connection string to use for the CosmosDB
param cosmosConnectionString string

// URL to the zip file containing the function to deploy
param packageUri string

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource site 'Microsoft.Web/sites@2020-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'functionapp'
  properties: {
    serverFarmId: servicePlanId
    clientAffinityEnabled: false
    siteConfig: {
      alwaysOn: true
      phpVersion: ''
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'databaseName'
          value: 'reaper'
        }
        {
          name: 'FUNCTONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsightsInstrumenationKey
        }
        {
          name: 'connectionString'
          value: cosmosConnectionString
        }
        {
          name: 'OpenApi__AuthLevel__UI'
          value: 'Function'
        }
      ]
    }
  }
}

resource siteExtensionMSDeploy 'Microsoft.Web/sites/extensions@2020-12-01' = {
  name: concat(name, '/MSDeploy')
  dependsOn: [
    site
  ]
  properties: {
    packageUri: packageUri
  }
}
