
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// id of the serviceplan
param servicePlanId string

// name of the storage account
param storageAccountName string

// access key for the storage account
param storageAccountKey string

// table ednpoint for storage
param storageTableEndpoint string

// the instrumentation key for application insights
param applicationInsightsInstrumenationKey string

// name of the cosmos database
param cosmosDbName string

// key to access the cosmosDb
param cosmosDbKey string

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
          value: concat('DefaultEndPointsProtocol=https;AccountName=', storageAccountName, ';AccountKey=', storageAccountKey, ';')
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
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsightsInstrumenationKey
        }
        {
          name: 'connectionString'
          value: concat('DefaultEndPointsProtocol=https;AccountName=', storageAccountName, ';AccountKey=', storageAccountKey, ';TabelEndpoint=', storageTableEndpoint, ';')
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
