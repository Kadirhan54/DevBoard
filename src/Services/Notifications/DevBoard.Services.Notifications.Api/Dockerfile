
# ============================================================================
# FILE: src/Services/Notifications/DevBoard.Services.Notifications.Api/Dockerfile
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY DevBoard.sln ./
COPY src/Shared/DevBoard.Shared.Contracts/DevBoard.Shared.Contracts.csproj src/Shared/DevBoard.Shared.Contracts/
COPY src/Shared/DevBoard.Shared.Common/DevBoard.Shared.Common.csproj src/Shared/DevBoard.Shared.Common/
COPY src/Services/Notifications/DevBoard.Services.Notifications.Api/DevBoard.Services.Notifications.Api.csproj src/Services/Notifications/DevBoard.Services.Notifications.Api/
COPY src/Services/Notifications/DevBoard.Services.Notifications.Infrastructure/DevBoard.Services.Notifications.Infrastructure.csproj src/Services/Notifications/DevBoard.Services.Notifications.Infrastructure/

RUN dotnet restore "src/Services/Notifications/DevBoard.Services.Notifications.Api/DevBoard.Services.Notifications.Api.csproj"

COPY src/ ./src/

WORKDIR /src/src/Services/Notifications/DevBoard.Services.Notifications.Api
RUN dotnet build "DevBoard.Services.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "DevBoard.Services.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBoard.Services.Notifications.Api.dll"]