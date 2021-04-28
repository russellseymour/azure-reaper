
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// name of the queue to use for alerts
param queueName string

// name of the connection to use for accessing the queue
param connectionName string

// the id of the connection to use
param apiConnectionId string

param apiId string

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource workflow 'Microsoft.Logic/workflows@2019-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    state: 'Enabled'
    parameters: {
      '$connections': {
        value: {
          'azurequeues': {
            connectionId: apiConnectionId
            connectionName: connectionName
            id: apiId
          }
        }
      }
    }
    definition: {
      '$schema': '#https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        manual: {
          type: 'Request'
          kind: 'Http'
          inputs: {
            schema: {}
          }
        }
      }
      actions: {
        'Create_a_new_queue': {
          runAfter: {}
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'azurequeue\'][\'connectionId\']'
              }
            }
            method: 'put'
            path: '/putQueue'
            queries: {
              queueName: queueName
            }
          }
        }
        'Put_a_message_on_a_queue': {
          runAfter: {
            'Create_a_new_queue': [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            body: '@{triggerBody()}'
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'azurequeue\'][\'connectionId\']'
              }
            }
            method: 'post'
            path: '/@{encodeURIComponent(\'logalertqueue\')}/messages'
          }
        }
      }
    }
  }
}

output id string = workflow.id
output callBackUrl string = listCallBackUrl(resourceId('Microsoft.Logic/workflows/triggers', name, 'manual'), '2019-05-01').value
