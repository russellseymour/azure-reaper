---
title: Settings
weight: 20
---

#### Reaper depends on a few settings. All of the settings are stored in the backend CosmosDB database, and some of them can be overridden by tags on the resource.

## Upload API Settings

Once the deployment has been completed the Reaper needs to be configured with some default settings.  A lot of these settings can be overridden using the user tags, but when these do not occur on a resource group there has to be a value that Reaper can use.

The main [github.com/russellseymour/azure-reaper](https://github.com/russellseymour/azure-reaper) repository has two files which contain the default values for the settings and the timezones.

1. https://github.com/russellseymour/azure-reaper/master/data/settings.json
1. https://github.com/russellseymour/azure-reaper/master/data/timezones.json

In order to access the API, the function key needs to be retrieved, please refer to the [API Authentication](/docs/API/authentication/) page for more information.

The easiest way to upload the necessary items is to use the command line. The following examples show how this can be done using both `curl` and `powershell`.

{{% notice note %}}
In both cases a dummy API token is being used for the authentication. The URL for the Azure Reaper endpoint will depend on where the function was deployed to and the parameters that were used
{{% /notice %}}

{{< tabs groupId="settings">}}
{{% tab name="Curl" %}}
```bash
# Download the two files to the local machine
curl https://github.com/russellseymour/azure-reaper/master/data/settings.json -o settings.json
curl https://github.com/russellseymour/azure-reaper/azure-reaper/master/data/timezones.json -o timezones.json

# Upload the settings to the API
curl -H "x-functions-key: functiontoken123!" \
     -X POST \
     -d @settings.json \
     https://azure-reaper.azurewebsites.net/ops/settings

# Upload the timezones to the API
curl -H "x-functions-key: functiontoken123!" \
     -X POST \
     -d @timezones.json \
     https://azure-reaper.azurewebsites.net/ops/timezones
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
# Download the two files to the local machine
Invoke-RestMethod -Uri https://github.com/russellseymour/azure-reaper/master/data/settings.json/master/data/settings.json -OutFile settings.json
Invoke-RestMethod -Uri https://github.com/russellseymour/azure-reaper/master/data/settings.json/master/data/timezones.json -OutFile timezones.json

# Upload the settings to the API
Invoke-RestMethod -Method POST `
                  -Headers @{"x-functions-key": "functiontoken123!"} `
                  -Body (Get-Content -Path settings.json -raw) `
                  -Uri "https://azure-reaper.azurewebsites.net/ops/settings"

# Upload the timezones to the API
Invoke-RestMethod -Method POST `
                  -Headers @{"x-functions-key": "functiontoken123!"} `
                  -Body (Get-Content -Path timezones.json -raw) `
                  -Uri "https://azure-reaper.azurewebsites.net/ops/timezones"

```
{{% /tab %}}
{{< /tabs >}}


