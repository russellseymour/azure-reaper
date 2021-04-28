// Define the parameters that this module needs to expose
param location string
param name string
param storageAccountType string = 'Standard_LRS'
param tags object

resource stg 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: name
  location: location
  sku:{
    name: storageAccountType
  }
  kind: 'StorageV2'
  tags: tags
}

output accesskey string = listKeys(stg.id, stg.apiVersion).keys[0].value
output connectionString string = concat('DefaultEndpointsProtocol=https;AccountName=', name, ';AccountKey=', listKeys(stg.id, stg.apiVersion).keys[0].value, ';')
// output tableEndpoint string = stg.properties.primaryEndpoints.table
