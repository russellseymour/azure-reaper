
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource insights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output resourceId string = insights.id
output instrumentationKey string = reference(insights.id).InstrumentationKey
