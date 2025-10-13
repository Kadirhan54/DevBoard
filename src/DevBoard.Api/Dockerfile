# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file
COPY src/DevBoard.Api/DevBoard.Api.csproj ./DevBoard.Api/
RUN dotnet restore ./DevBoard.Api/DevBoard.Api.csproj

# Copy everything else
COPY . .
WORKDIR "/src/DevBoard.Api"

RUN dotnet build "./DevBoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DevBoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Optional: Run EF Core migrations automatically on startup
# Requires Microsoft.EntityFrameworkCore.Design package installed
# Uncomment the line below if you want automatic migration
# ENTRYPOINT ["dotnet", "DevBoard.Api.dll", "ef", "database", "update", "&&", "dotnet", "DevBoard.Api.dll"]

ENTRYPOINT ["dotnet", "DevBoard.Api.dll"]