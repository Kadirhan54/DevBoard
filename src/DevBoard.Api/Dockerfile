# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for caching
COPY DevBoard.sln ./
COPY src/DevBoard.Api/DevBoard.Api.csproj src/DevBoard.Api/
COPY src/DevBoard.Application/DevBoard.Application.csproj src/DevBoard.Application/
COPY src/DevBoard.Infrastructure/DevBoard.Infrastructure.csproj src/DevBoard.Infrastructure/
COPY src/DevBoard.Domain/DevBoard.Domain.csproj src/DevBoard.Domain/

# Restore dependencies
RUN dotnet restore "src/DevBoard.Api/DevBoard.Api.csproj"

# Copy all source code (preserve folder structure)
COPY src/ ./

# Set working directory to API project
WORKDIR /src/DevBoard.Api

# Build
RUN dotnet build "DevBoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DevBoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DevBoard.Api.dll"]
