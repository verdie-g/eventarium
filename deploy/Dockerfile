# syntax=docker/dockerfile:1

ARG DOTNET_SDK_VERSION=11.0.100-preview.7
ARG ASPNET_VERSION=11.0.0-preview.7

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
ARG TARGETARCH
WORKDIR /source

# Blazor WebAssembly AOT compilation requires Python and the wasm-tools workload.
RUN apt-get update \
    && apt-get install --no-install-recommends --yes python3 \
    && rm -rf /var/lib/apt/lists/* \
    && dotnet workload install wasm-tools

# Restore project files as a separate layer so dependency downloads remain cached.
COPY --link global.json ./
COPY --link src/Directory.Build.props src/Directory.Packages.props src/Eventarium.slnx src/
COPY --link src/Eventarium.Client/*.csproj src/Eventarium.Client/
COPY --link src/Eventarium.Core/*.csproj src/Eventarium.Core/
COPY --link src/Eventarium.Providers.Fake/*.csproj src/Eventarium.Providers.Fake/
COPY --link src/Eventarium.Providers.GitHub/*.csproj src/Eventarium.Providers.GitHub/
COPY --link src/Eventarium.Server/*.csproj src/Eventarium.Server/
RUN dotnet restore src/Eventarium.Server/Eventarium.Server.csproj -a $TARGETARCH

COPY --link src/ src/
RUN dotnet publish src/Eventarium.Server/Eventarium.Server.csproj \
    -c Release \
    -a $TARGETARCH \
    --no-restore \
    --output /app \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS final
WORKDIR /app
EXPOSE 8080 8443
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Eventarium.Server.dll"]
