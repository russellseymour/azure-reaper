---
title: Infrastructure
weight: 10
---

Azure Reaper runs as an Azure function, which reads from queues and has Azure monitoring to inform when a new resource group has been created. The following diagram shows what needs to be deployed.

![Azure Reaper Infrastructure](/images/az_reaper_infrastructure.png)

The infrastructure can be deployed in one of three ways, using the Azure CLI, Azure PowerShell or Terraform.

{{< tabs groupId="deployment">}}
{{% tab name="Azure CLI" %}}
```bash
# Create the resource group into which the templates will be deployed
az group create -n azure-reaper -l uksouth

# Deploy the templates into the resource group
az deployment group create -g azure-reaper -f infrastructure/azuredeploy.json -p prefix=rjs
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
# Create the resource group into which the templates will be deployed
New-AzResourceGroup -Name azure-reaper -Location uksouth

# Deploy the templates into the resource group
New-AzResourceGroupDeployment -ResourceGroupName azure-reaper -TemplateFile infrastructure/azuredeploy.json -prefix rjs
```
{{% /tab %}}
{{% tab name="Terraform" %}}
```bash
# Change into the directory containing the templates
cd infrastructure

# Initialise terraform, to install the necessary providers
terraform init

# Plan and then run terraform
terraform plan --var 'resource_group_name=azure_reaper'
terraform apply --var 'resource_group_name=azure_reaper'
```
{{% /tab %}}
{{< /tabs >}}

Once the application has been deployed, it needs to be configured. Please refer to the [Settings]({{< ref "settings" >}} "Settings")