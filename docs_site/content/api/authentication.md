---
title: Authentication
weight: 10
---

#### Authentication is required whenever accessing the AZReaper API so that it cannot be accessed without authorization.

When accessing the API, authentication is required. This has to be set on the request using the `x-functions-key` header value. This value can be retrieved using the Azure Portal or from the command line using the Key Management API.

## Azure Portal

To get this navigate to the deployed application in the portal and select 'App Keys' from the left hand navigation.

![Function App Key](/images/az_reaper_func_key.png)

## Command Line

It is possible to use the `az` command line utility to retrieve the key for the function, however this will _only_ return the master key for the function.

_The following code snippet assumes a function name of `azure-reaper` in the `Azure-Reaper` resource group_.

{{< tabs groupId="api" >}}
{{% tab name="Azure CLI" %}}
```bash
az functionapp -n azure-reaper -g Azure-Reaper -o table
```

This will output something similar to the following:

```
MasterKey
--------------------------------------------------------
kDjlSiT9937WcSHaR3dn....
```

{{% /tab %}}
{{% tab name="PowerShell" %}}
There is no built in method in PowerShell to get the masterkey from a function app. However a function can be created that achieves the same result. Create a file called `Get-AzureFunctionsAccessToken.ps1` and copy in the following contents:

```powershell
param (
    [Parameter(Mandatory = $true)]
    [string]
    $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [string]
    $FunctionAppName
)

$publishingCredentials = Invoke-AzResourceAction  `
    -ResourceGroupName $ResourceGroupName `
    -ResourceType 'Microsoft.Web/sites/config' `
    -ResourceName ('{0}/publishingcredentials' -f $FunctionAppName) `
    -Action list `
    -ApiVersion 2019-08-01 `
    -Force

$base64Credentials = [Convert]::ToBase64String(
    [Text.Encoding]::ASCII.GetBytes(
        ('{0}:{1}' -f $publishingCredentials.Properties.PublishingUserName, $publishingCredentials.Properties.PublishingPassword)
    )
)

$jwtToken = Invoke-RestMethod `
    -Uri ('https://{0}.scm.azurewebsites.net/api/functions/admin/token' -f $FunctionAppName) `
    -Headers @{ Authorization = ('Basic {0}' -f $base64Credentials) }

# Get the tokens from the function
Invoke-RestMethod `
    -Uri ('https://{0}.azurewebsites.net/admin/host/keys' -f $FunctionAppName) `
    -Headers @{ Authorization =('Bearer {0}' -f $jwtToken)} `
    -Method GET | Select-Object -ExpandProperty keys
```

Now run the following commands:

```powershell
# Log into Azure
Login-AzAccount

# Get the token
./Get-AzureFunctionsAccessToken.ps1 -ResourceGroupName Azure-Reaper -FunctionAppName azreaper-5ytw
```

This will output something similar to the following:

```powershell
name    value
----    -----
default ZMeVN0c9MASECz1Tah4....
```

{{% /tab %}}
{{< /tabs >}}


