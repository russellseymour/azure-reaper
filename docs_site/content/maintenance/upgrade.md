---
title: Function Upgrade
weight: 20
vars:
    filename: azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip
---

New releases of Azure Reaper will be made available periodically which will provide bug fixes and new features. It is recommended that the releases are monitored to determine if an upgrade is necessary.

The commands shown here will upgrade the function to version {{< param "releaseUpgradeVersion" >}}. In order to perform the upgrade the following information is required. (The values in brackets are the ones that are used in the example commands).

 - Resource Group (`{{< param "resourceGroup" >}}`) - name of the resource group that Azure Reaper has been deployed into.
 - Function Name (`{{< param "functionName" >}}`) - name of the Azure function where the code is deployed to.

{{% notice note %}}
The commands shown here _only_ upgrade the code running in the Azure Function and do not touch any of the architecture. This means that Azure Reaper can be upgrade in situ without loosing existing settings or subscriptions.
{{% /notice %}}

{{< tabs groupId="deployment">}}
{{% tab name="Azure CLI" %}}
```bash
# Download the function zip file from the release page
curl https://{{< param "githubOrg" >}}/{{< param "repoName" >}}/releases/download/{{< param "releaseUpgradeVersion" >}}/azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip -o azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip

# Ensure logged into Azure
az login

# Deploy the updated function
az functionapp deployment source config-zip -g {{< param "resourceGroup" >}} -n {{< param "functionName" >}} --src azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip
```

{{% /tab %}}

{{% tab name="PowerShell" %}}
```powershell
# Download the function zip file from the release page
Invoke-RestMethod -Uri https://{{< param "githubOrg" >}}/{{< param "repoName" >}}/releases/download/{{< param "releaseUpgradeVersion" >}}/azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip -Outfile azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip

# Ensure logged into Azure
Connect-AZAccount

# Deploy the updated function
Publish-AzWebApp -ResourceGroupName {{< param "resourceGroup" >}} -Name {{< param "functionName" >}} -ArchivePath azure-reaper-{{< param "releaseUpgradeVersion" >}}.zip
```
{{% /tab %}}
{{< /tabs >}}

## Sample Output

The following screen shot shows the type of output to be expected from the command.

{{< figure src="/images/function_deploy_az_cli.png" title="Sample Output">}}