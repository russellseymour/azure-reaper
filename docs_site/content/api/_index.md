+++
title = "API"
date = 2021-04-09T14:50:07+01:00
weight = 20
chapter = true
pre = "<i class='fas fa-rocket'></i> "
+++

### Chapter 2

# API

Azure Reaper has an API that allows modifications to the Settings, Subscriptions and Timezones.

There are four main endpoints in the API.

| Endpoint | Description |
|---|---|
| `/api/v1/setting/{id?}` | Get or set a specific setting |
| `/api/v1/location/{id?}` | Get or set a location for timezone calculations |
| `/api/v1/subscription/{id?}` | Get or set subscriptions that need to be managed |
| `/api/v1/reaper` | Force a run of the reaper, independently of the timer trigger |
| `/api/swagger/ui` | Swagger UI for all the API endpoints |