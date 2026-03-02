# HR Hiring System - Azure Function

Azure Function for triggering AI evaluation of job applications in the background.

## Trigger Types

This function can be triggered via:
1. **HTTP Trigger** - Called by .NET API after job application is submitted
2. **Queue Trigger** (optional) - Can be set up with Azure Service Bus

## Setup

1. Install Azure Functions Core Tools
2. Create local.settings.json from template
3. Run locally: `func start`

## Environment Variables

- `AI_AGENT_URL` - URL of the Python FastAPI AI Agent
- `DOTNET_API_URL` - URL of the .NET API for fetching job details
