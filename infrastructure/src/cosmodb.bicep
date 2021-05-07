
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource db 'Microsoft.DocumentDB/databaseAccounts@2021-03-15' = {
  name: name
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    capabilities: [
      {
        name: 'EnableTable'
      }
    ]
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations:[
      {
        locationName: resourceGroup().location
        failoverPriority: 0
      }
    ]
  }
}

output key string = listKeys(db.id, db.apiVersion).primaryMasterKey
output connectionString string = concat('DefaultEndpointsProtocol=https;AccountName=', name, ';AccountKey=', listKeys(db.id, db.apiVersion).primaryMasterKey, ';TableEndpoint=https://', name, '.table.cosmos.azure.com:443/;')
