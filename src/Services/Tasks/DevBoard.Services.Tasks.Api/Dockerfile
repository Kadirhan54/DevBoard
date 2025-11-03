# ============================================================================
# FILE: src/Services/Tasks/DevBoard.Services.Tasks.Api/Dockerfile
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
COPY src/Services/Tasks/DevBoard.Services.Tasks.Api/DevBoard.Services.Tasks.Api.csproj src/Services/Tasks/DevBoard.Services.Tasks.Api/
COPY src/Services/Tasks/DevBoard.Services.Tasks.Core/DevBoard.Services.Tasks.Core.csproj src/Services/Tasks/DevBoard.Services.Tasks.Core/
COPY src/Services/Tasks/DevBoard.Services.Tasks.Infrastructure/DevBoard.Services.Tasks.Infrastructure.csproj src/Services/Tasks/DevBoard.Services.Tasks.Infrastructure/

RUN dotnet restore "src/Services/Tasks/DevBoard.Services.Tasks.Api/DevBoard.Services.Tasks.Api.csproj"

COPY src/ ./src/

WORKDIR /src/src/Services/Tasks/DevBoard.Services.Tasks.Api
RUN dotnet build "DevBoard.Services.Tasks.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "DevBoard.Services.Tasks.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBoard.Services.Tasks.Api.dll"]