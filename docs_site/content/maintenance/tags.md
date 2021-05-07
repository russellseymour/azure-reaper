---
title: Setting Tags
weight: 10
vars:
    resource_group: reap-test-1
---

Tags can be applied to the resource group or virtual machine using the Azure Portal, Azure CLI or PowerShell.

The easiest way to set the tags is to use the Azure Portal, however there are times when it is more convenient to set the tags using the Azure CLI or PowerShell. The following are some examples showing how to apply tags using the command line.

In all the examples the following values are used for the resources, these should be updated accordingly.
 - Subscription ID: `{{< param "subscriptionId" >}}`
 - Resource Group: `{{< param "resourceGroup" >}}`
 - Virtual Machine: `{{< param "virtualMachine" >}}`

#### Do not allow Reaper to manage the Resource Group

{{< tabs groupId="setting_inuse_tag" >}}
{{% tab name="Azure CLI" %}}
```bash
az tag create --resource-id /subscriptions/{{< param "subscriptionId" >}}/resourcegroups/{{< param "resourceGroup">}} --tags INUSE=true
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
Set-AzResourceGroup -Name {{< param "resourceGroup" >}} -Tag @{INUSE = "true"}
```
{{% /tab %}}
{{< /tabs >}}

#### Change the times that a specific machine should run

{{< tabs groupId="setting_runtime_tag" >}}
{{% tab name="Azure CLI" %}}
```bash
az vm update --resource-group {{< param "resourceGroup" >}} \
             --name {{< param "virtualMachine" >}} \
             --set tags.STARTSTOPTIME="1200-2000"
```
{{% /tab %}}
{{% tab name="PowerShell" %}}
```powershell
Set-AzResource -ResourceGroupName {{< param "resourceGroup" >}} `
               -Name {{< param "virtualMachine" >}} `
               -ResourceType "Microsoft.Compute/VirtualMachines" `
               -Tag @{STARTSTOPTIME = "1200-2000"}
```
{{% /tab %}}
{{< /tabs >}}