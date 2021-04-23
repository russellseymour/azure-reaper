variable "resource_group_name" {
    description = "Name of the resource group to create for the Azure Reaper"
}

variable "location" {
    description = "Resource location"
    default = "westeurope"
}

variable "deployment_name" {
    description = "Name of the deployment"
    default = "azure-reaper-deployment"
}