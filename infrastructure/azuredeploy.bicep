
// Parameters ---------------------------------------------------------------

// Set the URI for the zip file containing the the function to deploy
param packageUri string

// - preifx is used to name the resources that are deployed into Azure
param prefix string = 'AZReaper'

// - set the location. if not set this is based on the source group location
param location string = resourceGroup().location

// - define the individual components for the service plan sku
param spTier string = 'Standard'
param spName string = 'S1'
param spSize string = 'S1'
param spFamily string = 'S'
param spCapacity int = 1

// Variables ----------------------------------------------------------------

// create a unique name and a short version
var unique = uniqueString(subscription().id, resourceGroup().id, deployment().name)
var uniqueShort = substring(unique, 0, 4)

// create an object of names
var name = {
  actionGroup: concat(prefix, '-ActionGroup')
  actionGroupShortname: concat(prefix, '-ag')
  activityLogAlert: concat(prefix, '-ActivityLogAlert')
  apiConnection: 'AlertQueueConnection'
  applicationInsights: concat(prefix, '-AppInsights')
  appServicePlan: concat(prefix, '-ServicePlan')
  azureQueues: concat(prefix, '-APIConnection')
  cosmosdb: concat(toLower(prefix), '-', uniqueShort)
  logicApps: {
    alert: concat(prefix, '-LogAlert-LogicApp')
  }
  queue: 'logalertqueue'
  site: concat(toLower(prefix), '-', uniqueShort)
  storageAccount: unique
}

// create the object required to define the service plan SKU
var spSKU = {
  tier: spTier
  name: spName
  size: spSize
  family: spFamily
  capacity: spCapacity
}

// Resources ----------------------------------------------------------------

// Storage Account
module saModule 'src/storage-account.bicep' = {
  name: concat(name.storageAccount, '-Deployment')
  params: {
    location: location
    name: name.storageAccount
    tags: {
      description: 'Storage account used for table data used by Reaper'
    }
  }
}

// Cosmos DB
module cosmosDbModule 'src/cosmodb.bicep' = {
  name: concat(name.cosmosdb, '-Deployment')
  params: {
    location: location
    name: name.cosmosdb
    tags: {
      description: 'Data storage for settings, subscriptions and timezones'
    }
  }
}

// Application Insights
module appInsightsModule 'src/application-insights.bicep' = {
  name: concat(name.applicationInsights, '-Deployment')
  params: {
    location: location
    name: name.cosmosdb
    tags: {
      description: 'Monitoring for the functions website'
    }
  }
}

// AppService Plan
module appServicePlanModule 'src/app-service-plan.bicep' = {
  name: concat(name.appServicePlan, '-Deployment')
  params: {
    location: location
    name: name.cosmosdb
    tags: {
      description: 'Web server farm to host the function'
    }
    sku: spSKU
  }
}

// AppService
module appServiceModule 'src/app-service.bicep' = {
  name: name.site
  dependsOn: [
    saModule
    appInsightsModule
    appServicePlanModule
  ]
  params: {
    location: location
    name: name.cosmosdb
    tags: {
      description: 'Website app service to execute the Azure Reaper function'
    }
    servicePlanId: appServicePlanModule.outputs.id
    storageAccountName: name.storageAccount
    storageAccountKey: saModule.outputs.accesskey
    storageTableEndpoint: saModule.outputs.tableEndpoint
    applicationInsightsInstrumenationKey: appInsightsModule.outputs.instrumentationKey
    cosmosDbName: name.cosmosdb
    cosmosDbKey: cosmosDbModule.outputs.key
    packageUri: packageUri
  }
}

// Queue API Connection
module apiConnQueue 'src/apiConnections/azurequeue.bicep' = {
  name: concat(name.apiConnection, '-Deployment')
  params: {
    location: location
    name: name.apiConnection
    tags: {
      description: 'API Connection to the Storage Account for Logic Apps'
    }
    storageAccountName: name.storageAccount
    storageAccountKey: saModule.outputs.accesskey
  }
}

// Alert Logic App
module alertLogicApp 'src/logicApps/alert.bicep' = {
  name: concat(name.logicApps.alert, '-Deployment')
  params: {
    location: location
    name: name.logicApps.alert
    tags: {
      description: 'Logic App to recieve the alert that is generated when a new eresource group is created'
    }
    apiConnectionId: apiConnQueue.outputs.id
    queueName: name.queue
    connectionName: name.azureQueues
  }
}

// Action group
module actionGroup 'src/action-group.bicep' = {
  name: concat(name.actionGroup, '-Deployment')
  params: {
    location: location
    name: name.logicApps.alert
    tags: {
      description: 'Actions that need to be performed when a new resource group has been created'
    }
    logicAppCallbackUrl: alertLogicApp.outputs.callBackUrl
    logicAppId: alertLogicApp.outputs.id
    shortName: name.actionGroupShortname
  }
}

// Activity Log alert
module activityLogAlert 'src/activity-logalert.bicep' = {
  name: concat(name.activityLogAlert, '-Deployment')
  params: {
    location: location
    name: name.logicApps.alert
    tags: {
      description: 'Alert generated when a new resource group is created'
    }
    actionGroupId: actionGroup.outputs.id
  }
}
