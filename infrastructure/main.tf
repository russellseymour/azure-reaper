
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.52.0"
    }
  }
}

provider "azurerm" {
    features {}
}

# Create the resource group into which the arm template will be deployed
resource "azurerm_resource_group" "rg" {
    name = var.resource_group_name
    location = var.location
}

resource "azurerm_resource_group_template_deployment" "azure_reaper_infrastructure" {
    name = var.deployment_name
    resource_group_name = azurerm_resource_group.rg.name
    deployment_mode = "Incremental"
    template_content = file("azuredeploy.json")

    parameters_content = file("parameters.json")
}