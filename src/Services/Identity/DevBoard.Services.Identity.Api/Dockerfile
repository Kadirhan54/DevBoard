# ============================================================================
# FILE: src/Services/Identity/DevBoard.Services.Identity.Api/Dockerfile
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY DevBoard.sln ./

# Copy all project files
COPY src/Shared/DevBoard.Shared.Contracts/DevBoard.Shared.Contracts.csproj src/Shared/DevBoard.Shared.Contracts/
COPY src/Shared/DevBoard.Shared.Common/DevBoard.Shared.Common.csproj src/Shared/DevBoard.Shared.Common/
COPY src/Services/Identity/DevBoard.Services.Identity.Api/DevBoard.Services.Identity.Api.csproj src/Services/Identity/DevBoard.Services.Identity.Api/
COPY src/Services/Identity/DevBoard.Services.Identity.Core/DevBoard.Services.Identity.Core.csproj src/Services/Identity/DevBoard.Services.Identity.Core/
COPY src/Services/Identity/DevBoard.Services.Identity.Infrastructure/DevBoard.Services.Identity.Infrastructure.csproj src/Services/Identity/DevBoard.Services.Identity.Infrastructure/

# Restore dependencies
RUN dotnet restore "src/Services/Identity/DevBoard.Services.Identity.Api/DevBoard.Services.Identity.Api.csproj"

# Copy all source code
COPY src/ ./src/

# Build
WORKDIR /src/src/Services/Identity/DevBoard.Services.Identity.Api
RUN dotnet build "DevBoard.Services.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DevBoard.Services.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBoard.Services.Identity.Api.dll"]