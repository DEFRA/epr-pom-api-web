# EPR Web API Gateway

## Overview

Web API Gateway is the frontend's access gateway to submission upload status and anti-virus services.

## How To Run

### Prerequisites

In order to run the service you will need the following dependencies

- .NET 6

#### epr-packaging-common
##### Developers working for a DEFRA supplier
In order to restore and build the source code for this project, access to the `epr-packaging-common` package store will need to have been setup.
 - Login to Azure DevOps
 - Navigate to [Personal Access Tokens](https://dev.azure.com/defragovuk/_usersSettings/tokens)
 - Create a new token
   - Enable the `Packaging (Read)` scope
Add the following to your `src/Nuget.Config`
```xml
<packageSourceCredentials>
  <epr-packaging-common>
    <add key="Username" value="<email address>" />
    <add key="ClearTextPassword" value="<personal access token>" />
  </epr-packaging-common>
</packageSourceCredentials>
```
##### Members of the public
Clone the [epr_common](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr_common) repository and add it as a project to the solution you wish to use it in. By default the repository will reference the files as if they are coming from the NuGet package. You simply need to update the references to make them point to the newly added project.

### Run

 On WebAPIGateway.API directory, execute

```
dotnet run
```

### Docker

Generate a PAT by following the steps here - [Generate Personal Access Token](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows#create-a-pat),

Then run in terminal at the WebAPIGateway solution root (EPR_POM_API_WEB/WebAPIGateway)

```
docker build -t webapigateway -f WebAPIGateway.API/Dockerfile --build-arg PAT={YOUR PAT HERE} .
```

Then after that command has completed run:

```
docker run -p 5291:3000 --name webapigatewaycontainer webapigateway   
```

Do a GET Request to ```http://localhost:5291/admin/health``` to confirm that the service is running

## How To Test

### Unit tests

On root directory, execute

```
dotnet test
```

### Pact tests

N/A

### Integration tests

N/A

## How To Debug

N/A

## Environment Variables - deployed environments

The structure of the appsettings can be found in the repository. Example configurations for the different environments can be found in [epr-app-config-settings](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr-app-config-settings).

| Variable Name                    | Description                                                                |
|---------------------------------|----------------------------------------------------------------------------|
| AccountApi__BaseUrl              | Account Facade base URL                                                   |
| AccountApi__ClientId             | Account API client ID                                                     |
| AccountApi__Timeout              | Number of seconds before timing out request to Account API                |
| SubmissionStatusApi__BaseUrl     | Submission Status API base URL                                            |
| AntivirusApi__BaseUrl            | Antivirus API base URL                                                    |
| AntivirusApi__SubscriptionKey    | Antivirus API APIM subscription key                                       |
| AntivirusApi__TenantId           | Antivirus API APIM tenant ID                                              |
| AntivirusApi__ClientId           | Antivirus API APIM client ID                                              |
| AntivirusApi__ClientSecret       | Antivirus API APIM client secret                                          |
| AntivirusApi__Scope              | Antivirus API APIM scope                                                  |
| AntivirusApi__Timeout            | Number of seconds before timing out request to the Antivirus API          |
| AntivirusApi__CollectionSuffix   | CollectionSuffix is appended to the collection name passed to the Antivirus API. It allows the Antivirus API to support message filtering on each subscription used in the different dev environments. |
| AzureAdB2C__Instance             | Azure B2C instance                                                        |
| AzureAdB2C__Domain               | Azure B2C domain                                                          |
| AzureAdB2C__ClientId             | Azure B2C client ID                                                       |
| AzureAdB2C__ClientSecret         | B2C client secret                                                         |
| LoggingApi__BaseUrl              | Logging API base URL                                                      |
| StorageAccount__PomContainer     | Container name on the storage account where POM files are stored. This value is used to populate the field ContainerName on CosmosDB items. |
| StorageAccount__RegistrationContainer | Container name on the storage account where registration files are stored. This value is used to populate the field ContainerName on CosmosDB items. |

## Additional Information

[ADR-001: Authentication and Authorization](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4170645514/ADR-001+Authentication+and+Authorization+draft)

[ADR-012.A: EPR Phase 1 - Compliance Scheme PoM Data Upload](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4251418625/ADR-012.A+EPR+Phase+1+-+Compliance+Scheme+PoM+Data+Upload)

### Logging into Azure

N/A

### Usage

N/A

### Monitoring and Health Check

Health check - ```{environment}/admin/health```

## Directory Structure

### Source files

- `WebAPIGateway/WebAPIGateway.Api` - API .NET source files
- `WebAPIGateway/WebAPIGateway.UnitTests` - API .NET unit test files
- `WebAPIGateway/WebAPIGateway.Core` - Data .NET source files

## Contributing to this project

Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).
