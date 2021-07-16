---
title: Enable cross subscription tagging and reaping
weight: 10
---

The Azure Reaper was always built to handle tagging and managing resources in different subscriptions, however to do so requires some additional setup. This guide details how this is done and what extra credentials and resources are required.

## Credentials

Azure Reaper works by utilising Service Principals (SPN) which give it permission to work within the specific subscription. This needs to be created with the Contributor role so that it can enumerate the resource groups for tagging and manage the running state of virtual machines and Kubernetes clusters.

{{< tabs groupId="spn">}}
{{% tab name="Azure CLI" %}}
```bash
az ad sp create-for-rbac --name AzureReaper --role Contributor
```

```json
{
  "appId": "4703916a-c745-427e-9559-27488be86dfd",
  "displayName": "AzureReaper",
  "name": "http://AzureReaper",
  "password": "5lj02Jw1awGO6-UWG.7cLplhxRTeM3Ix0d",
  "tenant": "f88c76e1-2e79-4cd5-8b37-842f3f870d58"
}
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
New-AzADServicePrincipal -DisplayName "AzureReaper" -Role "Contributor"

# Output
Secret                : System.Security.SecureString
ServicePrincipalNames : {41fcd592-5fb9-40a1-a8cc-e0904833edd2, http://AzureReaper}
ApplicationId         : 41fcd592-5fb9-40a1-a8cc-e0904833edd2
ObjectType            : ServicePrincipal
DisplayName           : AzureReaper
Id                    : 70a2a996-c2f9-4c03-b6db-aaab36d4b7e1
```
{{% /tab %}}
{{< /tabs >}}

After obtaining the service principal credentials, Azure Reaper needs to be updated with this information. The easiest way to do this is using the API, with the token for the function.

### Get the function Key

In order to use the API, the function key is required. This can be done on the command line.

{{< tabs groupId="functionkey">}}
{{% tab name="Azure CLI" %}}

```bash
az functionapp function keys list -n azreaper-zs4t -g Azure-Reaper --function-name SubscriptionUpsert -o table

Default
--------------------------------------------------------
/D8/huKbKwQo5fg6atPgaihL0jXaY5d5zB6gxe/k/KjtK6OV2nqxrg==
```

{{% /tab %}}
{{% tab name="PowerShell" %}}


{{% /tab %}}
{{% /tabs %}}

### Add the subscription to Reaper

The following command line should how to add the new subscription to Reaper using the API.

{{< tabs groupId="subscriptionApi">}}
{{% tab name="Azure CLI" %}}

```bash
curl -X POST -H "x-functions-key: /D8/huKbKwQo5fg6atPgaihL0jXaY5d5zB6gxe/k/KjtK6OV2nqxrg==" \
  -d '{"partitionKey": "subscriptions", "client_id": "4703916a-c745-427e-9559-27488be86dfd", \
  "client_secret": "5lj02Jw1awGO6-UWG.7cLplhxRTeM3Ix0d", "tenantId": \ "f88c76e1-2e79-4cd5-8b37-842f3f870d58", "subscriptionId": "45ac53dc-0480-4b01-95c7-cffd067d2a7d", \
  "dryrun": false, "enabled": true, "name": "New Subscription", "reaper": false}' \
  https://azreaper-4zst.azurewebsites.net/api/v1/subscription
```

{{% /tab }}
{{% tab name="PowerShell" %}}

```powershell
$data = @{
  partitionKey = "subscriptions"
  client_id = "4703916a-c745-427e-9559-27488be86dfd"
  client_secret = "5lj02Jw1awGO6-UWG.7cLplhxRTeM3Ix0d"
  tenantId = "f88c76e1-2e79-4cd5-8b37-842f3f870d58"
  subscriptionId = "45ac53dc-0480-4b01-95c7-cffd067d2a7d"
  dryrun = $false
  enabled = $true
  name = "New Subscription"
  reaper = $false
}

# Convert the data into JSON
$body = $data | ConvertTo-Json

# Post to the API
Invoke-RestMethod -Method Post `
  -Uri https://azreaper-4zst.azurewebsites.net/api/v1/subscription `
  -Headers @{x-functions-key: "/D8/huKbKwQo5fg6atPgaihL0jXaY5d5zB6gxe/k/KjtK6OV2nqxrg=="} `
  -Body $body
```

{{% /tab %}}
{{% /tabs %}}

## Activity Alert

Azure Reaper works by using Activity Alerts that inform it when a new resource group has been created in the subscription. In order to get Azure Reaper to react to such an event an Activty Alert needs to be setup in the target subscription and pointed to the Logic App in the subscription in which Azure Reaper has been deployed.

The file `https://raw.githubusercontent.com/russellseymour/azure-reaper/master/infrastructure/src/activity-logalert.bicep` can be used to deploy a new Log Alert to the subscription.

{{% notice note %}}
This file is used by the main deployment, but it can be used as a standalone artefact as well.
{{% /notice %}}

The following commands show how to deploy this into the subscription. The subscription ID can be passed in on the command line, but the default is to use the subscription that is being deployed to. The `actionGroupId` which is what the alert is sent to, needs to be specified on the command line.

{{< tabs groupId="logalert">}}
{{% tab name="Azure CLI" %}}

Get the ID of action group that needs to be targeted by the Activity Alert.

```bash
az monitor action-group show -g Azure-Reaper -n AZReaper-LogAlert-LogicApp --query id -o table

Result
----------------------------------------------------------------------------------------------------------------------------------------------------
/subscriptions/xxxx/resourceGroups/Azure-Reaper/providers/microsoft.insights/actionGroups/AZReaper-LogAlert-LogicApp
```

Use the ID of the action group in the deployment

```bash
# Create the resource group into which the monitor will be deployed
az group create -n AzureReaper -l uksouth

# Deploy the Activity Alert
az deployment group create -g AzureReaper \
    --template-uri https://raw.githubusercontent.com/russellseymour/azure-reaper/master/infrastructure/src/activity-logalert.bicep \
    --parameters actionGroupId="/subscriptions/xxxx/resourceGroups/Azure-Reaper/providers/microsoft.insights/actionGroups/AZReaper-LogAlert-LogicApp"
    
```
{{% /tab %}}
{{% tab name="PowerShell" %}}

Get the ID of action group that needs to be targeted by the Activity Alert.

```powershell
(Get-AzActionGroup -ResourceGroup Azure-Reaper -Name AZReaper-LogAlert-LogicApp).Id
/subscriptions/xxxx/resourceGroups/Azure-Reaper/providers/microsoft.insights/actionGroups/AZReaper-LogAlert-LogicApp
```

Use the ID of the action group in the deployment

```powershell
# Create the resource group into which the monitor will be deployed
New-AzResourceGroup -Name Azure-Reaper -Location uksouth

# Deploy the Activity Alert
New-AzResourceGroupDeployment -ResourceGroupName Azure-Reaper `
    -TemplateUri "https://raw.githubusercontent.com/russellseymour/azure-reaper/master/infrastructure/src/activity-logalert.bicep" `
    -TemplateParameterObject @{"actionGroupId" = "/subscriptions/xxxx/resourceGroups/Azure-Reaper/providers/microsoft.insights/actionGroups/AZReaper-LogAlert-LogicApp"}

```

{{% /tab %}}
{{< /tabs >}}


