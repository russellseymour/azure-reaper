
// Parameters ---------------------------------------------------------------

// name of the CosmosDB in Azure
param name string

// location of the database
param location string

// tags that should be applied
param tags object

// short name of the actionGroup
param shortName string

// id of the logic app for alerts
param logicAppId string

// callback url to use
param logicAppCallbackUrl string

// Variables ----------------------------------------------------------------

// Resources ----------------------------------------------------------------

resource actionGroup 'microsoft.insights/actionGroups@2019-06-01' = {
  name: name
  location: location
  tags: tags
  
  properties: {
    groupShortName: toLower(shortName)
    enabled: true
    logicAppReceivers: [
      {
        name: 'LogicAppNotify'
        resourceId: logicAppId
        callbackUrl: logicAppCallbackUrl
        useCommonAlertSchema: true
      }
    ]
  }
}

output id string = actionGroup.id
