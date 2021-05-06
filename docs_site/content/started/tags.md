---
title: Tags
weight: 40
---

Tags are at the heart of how Azure Reaper operates. Although there are settings that govern globally how resource groups and virtual machines will be managed, the use of tags can customise this behaviour.

## Default Tags

The following tags are added to a resource group when it is created.

| Tag | Description | Example |
|---|---|---|
| `createdDate` | UTC Datestamp of when the group was created | `2021-05-04T13:27:01Z`|
| `owner` | Name of the person that created the group | Russell Seymour |
| `ownerEmail` | Email address of the owner | russell.seymour@azurereaper.com |

{{% notice warn %}}
Azure Reaper adds tags to the resource group by receiving an event from Azure to state is has been created. As it is event based it is not immediate and there can be a delay between when a group is created and the tags being applied.
{{% /notice %}}

## User Tags

A number of tags can be applied to a resource group or virtual machine to override the default settings. The following table shows the tags that can be applied and where.

| Tag | Description | Resource Group | Virtual Machine | Default |
|---|---|---|---|---|
| `DAYSOFWEEK` | Days of the week that machines are permitted to run on. This is a comma delimited list of numbers where Sunday is 0 and Saturday is 6 | <i class='fa fa-times-circle'></i> | <i class='fa fa-check-circle'></i> | 1,2,3,4,5 |
| `EXPIRED` | State if the resource group has already expired and Reaper should delete it on the next run. The presence of the tag is required, the value is redundant. | <i class='fa fa-check-circle'></i> | <i class='fa fa-times-circle'></i> | | 
| `INUSE` | States that the resource group is in use and should no be managed by Reaper. This is designed for applications that need to be kept running within a subscription | <i class='fa fa-check-circle'></i> | <i class='fa fa-check-circle'></i> | |
| `STARTSTOPTIME` | Start and stop time of virtual machines on permitted days. Azure Reaper calculates the timezone to work in from the location or the `TIMEZONE` tag. If applied to the group then all machines are affected, or individually if applied to a machine. This should be specified in the format `<STARTTIME> - <STOPTIME>` | <i class='fa fa-check-circle'></i> | <i class='fa fa-check-circle'></i> | `0800 - 1800` |
| `TIMEZONE` | Timezone that the resource should be managed in. If not specified then the Azure location will be used to determine the timezone. It is possible to have machines in different timezones | <i class='fa fa-check-circle'></i> | <i class='fa fa-check-circle'></i> | |
