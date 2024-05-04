FROM defradigital/dotnetcore-development:dotnet6.0 AS build-env

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy files
COPY --chown=dotnet WebApiGateway.Api/. ./WebApiGateway.Api/.
COPY --chown=dotnet WebApiGateway.Core/. ./WebApiGateway.Core/.
COPY --chown=dotnet NuGet.Config ./
COPY --chown=dotnet Directory.Build.props ./

# Use build argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><defra-epr-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></defra-epr-common></packageSourceCredentials></configuration>|" NuGet.Config

# Restore as distinct layers
RUN dotnet restore "WebApiGateway.Api/WebApiGateway.Api.csproj" --configfile "NuGet.Config"

# Build and publish a release
WORKDIR /home/dotnet/WebApiGateway.Api
RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet6.0

# Switch to non-root user
USER dotnet

COPY --from=build-env --chown=dotnet /home/dotnet/WebApiGateway.Api/out .

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet WebApiGateway.Api.dll
