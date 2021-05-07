
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// sku for the plan
param sku object

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource servicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: name
  location: location
  tags: tags
  sku: sku
}

output id string = servicePlan.id
