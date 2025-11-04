# syntax=docker/dockerfile:1.7

###
# Build stage (Apple Silicon ready)
###
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

# Copy solution
COPY *.sln ./

# Copy project files first for restore caching (note the GabsHybridApp/ prefix)
COPY GabsHybridApp/GabsHybridApp.Web/GabsHybridApp.Web.csproj GabsHybridApp/GabsHybridApp.Web/
COPY GabsHybridApp/GabsHybridApp.Shared/GabsHybridApp.Shared.csproj GabsHybridApp/GabsHybridApp.Shared/

# Restore the Web project (which references Shared)
RUN dotnet restore GabsHybridApp/GabsHybridApp.Web/GabsHybridApp.Web.csproj --arch $TARGETARCH

# Copy full sources
COPY GabsHybridApp/GabsHybridApp.Web/ GabsHybridApp/GabsHybridApp.Web/
COPY GabsHybridApp/GabsHybridApp.Shared/ GabsHybridApp/GabsHybridApp.Shared/

# Publish
RUN dotnet publish GabsHybridApp/GabsHybridApp.Web/GabsHybridApp.Web.csproj \
    -c Release -o /app/out \
    -p:UseAppHost=false \
    -p:PublishReadyToRun=true

###
# Runtime stage
###
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

RUN apk add --no-cache icu-libs tzdata libgcc \
    && adduser -D -H -u 10001 appuser

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=build /app/out ./

USER appuser
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget -qO- http://127.0.0.1:8080/ || exit 1

ENTRYPOINT ["dotnet", "GabsHybridApp.Web.dll"]
