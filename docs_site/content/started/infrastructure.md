---
title: Infrastructure
weight: 10
---

Azure Reaper runs as an Azure function, which reads from queues and has Azure monitoring to inform when a new resource group has been created. The following diagram shows what needs to be deployed.

![Azure Reaper Infrastructure](/images/az_reaper_infrastructure.png)

The infrastructure can be deployed using the Azure CLI or PowerShell. The template deploys the function version associated with the template.

{{% notice note %}}
The examples shown in the following code snippets use version {{< param "releaseVersion" >}} of Azure Reaper.
Please replace this value if you wish to deploy a different version.
{{% /notice %}}

{{< tabs groupId="deployment">}}
{{% tab name="Azure CLI" %}}
```bash
# Create the resource group into which the templates will be deployed
az group create -n azure-reaper -l uksouth

# Deploy the templates into the resource group
az deployment group create -g azure-reaper -templateUri https://{{< param "githubOrg" >}}/{{< param "repoName" >}}/releases/download/v{{< param "releaseVersion" >}}/azuredeploy.json
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
# Create the resource group into which the templates will be deployed
New-AzResourceGroup -Name azure-reaper -Location uksouth

# Deploy the templates into the resource group
New-AzResourceGroupDeployment -ResourceGroupName azure-reaper -TemplateUri https://{{< param "githubOrg" >}}/{{< param "repoName" >}}/releases/download/v{{< param "releaseVersion" >}}/azuredeploy.json
```
{{% /tab %}}
{{< /tabs >}}

Once the application has been deployed, it needs to be configured. Please refer to the [Settings]({{< ref "settings" >}} "Settings")