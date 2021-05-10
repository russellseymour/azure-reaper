
# Azure Reaper

The Azure Reaper is used to ensure that machines are not running needlessly and to delete resources when they are no longer required. In order to do this the Reaper automatically tags new resource groups that are created with with the name of the person that created it and the date and time of creation.

This repository contains the infrastructure templates for setting up the necessary resources in Azure, the function that performs the work and the documentation for the Reaper.

## Documentation

The main documentation can be found online at https://www.azurereaper.com.

The raw documentation is located in the `docs_site` directory. This folder contains the files required to generate the external website using [Hugo](https://gohugo.io).

To run the documentation locally, ensure that the Hugo binary is installed on the local machine and run the following commands

```bash
cd docs_site
hugo serve
```

## Deployment

NOTE: This section give quick start information about how to get Azure Reaper up and running. For more detailed information please refer to the main documentation.

THe quickest way to deploy Reaper is to use the 

# Emulators

Using emulators helps with the development of the application as it means that the Storage Account and Cosmos DB Account do not need exist in Azure for the code to be worked on.

[Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)
[Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)

# Running CosmosDB emulator

Start the emulator with the TableAPI enabled

```
Microsoft.Azure.Cosmos.Emulator.exe /EnableTableEndpoint
```

Default backend is Cosmos Table API

# Multi database support

Multi database support is enabled in the application, although for the moment only the CosmosTableAPI is supported.

# Improvements

- All of the database calls are baked into the model. Ideally this should be part of an Interface so that it can be swapped out as needed.

- Need to work out the logger that the default trigger is using, seem to have two running, the default one plus Serilog

    This can be seen on the output when functions are called

```
For detailed output, run func with --verbose flag.
[15:36:45 DBG] Not creating database as it is not required for this backend type
[2021-03-14T15:36:45.087Z] Executing 'Setting' (Reason='This function was programmatically called via the host APIs.', Id=63fcc541-3348-4c1d-9f9a-d471b27cfa7d)
[15:36:45 INF] C# HTTP trigger function processed a request.
[15:36:45 FTL] Unable to determine model type
[2021-03-14T15:36:45.266Z] Executed 'Setting' (Succeeded, Id=63fcc541-3348-4c1d-9f9a-d471b27cfa7d, Duration=207ms)
[2021-03-14T15:36:46.461Z] Host lock lease acquired by instance ID '0000000000000000000000004A35AF63'.
```

    The messages with the log level are from SeriLog and the others are the default one

- Instructions need to include information about how to setup the SPN for the subscription