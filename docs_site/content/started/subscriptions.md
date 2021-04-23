---
title: Subscriptions
weight: 30
---

#### A list of subscriptions that Reaper can work on has to be supplied. This is done by POST'ing to the API.

In order to Reaper to be able to tag and reap resources, it needs to be able to access the subscription in which the resource group has been created. To do this is needs to have an SPN to login with.

{{% notice warning %}}
Currently the SPN details are held in the deployed CosmosDB in clear text. This will be modified in a future release to store such information in a Key Vault
{{% /notice %}}

## Create SPN

The easiest way to create the SPN is to use either the Azure CLI or PowerShell.

{{< tabs groupId="spn">}}
{{% tab name="Azure CLI" %}}
```bash
az ad sp create-for-rbac --name AzureReaper
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
New-AzADServicePrincipal -DisplayName "AzureReaper"

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


## Add subscription to the API

In both cases, the commands above will output information about the SPN that has been created.

{{% notice tip %}}
In the following example the SPN created by the `az` command has been used
{{% /notice %}}

Using a text editor, create a JSON object that will be used to send the data to the API. The JSON object should be similar to the following:

```json
{
    "name": "RJS Visual Studio Subscription",
    "subscription_id": "28f3bff4-25d1-4ff1-974b-a48d53929c84",
    "client_id": "4703916a-c745-427e-9559-27488be86dfd",
    "client_secret": "5lj02Jw1awGO6-UWG.7cLplhxRTeM3Ix0d",
    "tenant_id": "f88c76e1-2e79-4cd5-8b37-842f3f870d58",
    "enabled": true,
    "reaper": false
}
```

Assuming this file has been saved as `subscription.json` and the API key has been retrieved for [Authentication](/docs/API/authentication/), the command to add to the API is as follows

{{< tabs groupId="addSubscription">}}
{{% tab name="Curl" %}}
```bash
curl -H "x-functions-key: functiontoken123!" \
     -X POST \
     -d @subscription.json \
     https://azure-reaper.azurewebsites.net/ops/subscription
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
Invoke-RestMethod -Method POST `
                  -Headers @{"x-functions-key": "functiontoken123!"} `
                  -Body (Get-Content -Path subscription.json -raw) `
                  -Uri https://azure-reaper.azurewebsites.net/ops/subscription
```
{{% /tab %}}
{{< /tabs >}}

## Subscription object properties

| Property | Description |
|---|---|
| name | Name of the subscription as referred to in Reaper |
| subscription_id | ID of the subscription that is to be managed |
| client_id | App ID of the SPN that has been created |
| client_secret | Secret associated with the specified App ID |
| tenant_id | Azure tenant that the subscription belongs to |
| enabled | Whether reaper is enabled, when set to `true` auto tagging will work |
| reaper | State if the reaping option should be enabled |