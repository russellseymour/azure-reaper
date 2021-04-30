
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// tags that should be applied
param tags object

// id of the action group
param actionGroupId string

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource logAlert 'Microsoft.Insights/activityLogAlerts@2020-10-01' = {
  name: name
  location: 'Global'
  tags: tags
  properties: {
    enabled: true
    description: 'Activity alert to watch for newly created resource groups'
    scopes: [
      subscription().id
    ]
    condition: {
      allOf: [
        {
          field: 'category'
          equals: 'Administrative'
        }
        {
          field: 'operationName'
          equals: 'Microsoft.Resources/subscriptions/resourcegroups/write'
        }
        {
          field: 'status'
          equals: 'Succeeded'
        }
      ]
    }
    actions: {
      actionGroups: [
        {
          actionGroupId: actionGroupId
          webhookProperties: {}
        }
      ]
    }
  }
}
