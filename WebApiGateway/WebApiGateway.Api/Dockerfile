FROM defradigital/dotnetcore-development:dotnet10.0 AS build-env
USER root

# Copy only the project files first, so the restore layer is cached
# independently of source changes
COPY NuGet.Config Directory.Build.props ./
COPY WebApiGateway.Api/WebApiGateway.Api.csproj ./WebApiGateway.Api/
COPY WebApiGateway.Core/WebApiGateway.Core.csproj ./WebApiGateway.Core/

# Insert the PAT into the Nuget file, restore, then delete the file in the same
# layer so the credential is not left behind in the build stage filesystem
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><defra-epr-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></defra-epr-common></packageSourceCredentials></configuration>|" NuGet.Config \
    && dotnet restore "WebApiGateway.Api/WebApiGateway.Api.csproj" --configfile "NuGet.Config" \
    && rm NuGet.Config

# Copy the remaining source
COPY WebApiGateway.Api/. ./WebApiGateway.Api/.
COPY WebApiGateway.Core/. ./WebApiGateway.Core/.

# Build and publish a release
WORKDIR /home/dotnet/WebApiGateway.Api
RUN dotnet publish -c Release --no-restore -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet10.0

# Switch to non-root user
USER dotnet
WORKDIR /home/dotnet

ARG BUILD_NUMBER
ARG GIT_SHA
ENV BUILD_NUMBER=$BUILD_NUMBER
ENV GIT_SHA=$GIT_SHA

# No --chown: files stay root-owned so the dotnet runtime user can read and
# execute them but not modify them (Sonar docker:S6504)
COPY --from=build-env /home/dotnet/WebApiGateway.Api/out .

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Exec form so dotnet is PID 1 and receives SIGTERM directly
CMD ["dotnet", "WebApiGateway.Api.dll"]
