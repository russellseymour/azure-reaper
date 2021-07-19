
// Parameters ---------------------------------------------------------------

// name of the Activity Alert in Azure
param name string

// tags that should be applied
param tags object

// id of the action group
param actionGroupId string

// The subscription that the logalert should be applied to
param subscriptionId string = subscription().id

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
      subscriptionId
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
