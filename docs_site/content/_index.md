---
title: "Azure Reaper"
---

# Azure Reaper

Azure Reaper is a way of reducing costs in Azure subscriptions as well as automatically tagging ownership of resource groups.

Azure is a great place to play around with new things and test deployments of code on infrastructure, however more often than not these resources are not deleted so they incur an ongoing cost, such as a virtual machines and Kubernetes nodes.

Reaper helps to keep down costs by:

 - Shutting down machines when they are not being used, and starting them up again
 - Deleting resources that have expired

Azure Reaper runs as an Azure Function and has the following main components.

{{< row >}}
{{% column class="border-green" %}}
## Tagger

Whenever a new resource group is created an event is created which is put onto a queue. This message is read by Azure Reaper and uses the information to tag the group with:

 - Owner Name
 - Owner Email
 - Creation Date

![Tagged Resource Group](/images/resource_group_tagged.png)

This helps to identify who has owns the group and allows the Reaper to determine when the group has expired and should be deleted.

{{% /column %}}
{{% column class="border-blue" %}}
## Timer

Every 10 minutes, by default, the reaper runs to determine which resources need to be shutdown or deleted.

The Reaper function can also be triggered manually nby using the API.
{{% /column %}}
{{% column class="border-purple" %}}
## API

All of the configuration for Reaper is stored in a storage account using the Azure CosmosDB API. These settings can be managed using the Reaper API. It also has a Swagger UI.

![Swagger UI](/images/swagger.png)

Additional functions, such as the Reaper, can be called on an ad-hoc basis.
{{% /column %}}
{{< /row >}}