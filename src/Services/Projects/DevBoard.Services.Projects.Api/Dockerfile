# ============================================================================
# FILE: src/Services/Projects/DevBoard.Services.Projects.Api/Dockerfile
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
COPY src/Services/Projects/DevBoard.Services.Projects.Api/DevBoard.Services.Projects.Api.csproj src/Services/Projects/DevBoard.Services.Projects.Api/
COPY src/Services/Projects/DevBoard.Services.Projects.Core/DevBoard.Services.Projects.Core.csproj src/Services/Projects/DevBoard.Services.Projects.Core/
COPY src/Services/Projects/DevBoard.Services.Projects.Infrastructure/DevBoard.Services.Projects.Infrastructure.csproj src/Services/Projects/DevBoard.Services.Projects.Infrastructure/

RUN dotnet restore "src/Services/Projects/DevBoard.Services.Projects.Api/DevBoard.Services.Projects.Api.csproj"

COPY src/ ./src/

WORKDIR /src/src/Services/Projects/DevBoard.Services.Projects.Api
RUN dotnet build "DevBoard.Services.Projects.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "DevBoard.Services.Projects.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBoard.Services.Projects.Api.dll"]