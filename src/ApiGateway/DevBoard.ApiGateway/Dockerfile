# ============================================================================
# FILE: src/ApiGateway/DevBoard.ApiGateway/Dockerfile
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY DevBoard.sln ./
COPY src/ApiGateway/DevBoard.ApiGateway/DevBoard.ApiGateway.csproj src/ApiGateway/DevBoard.ApiGateway/

RUN dotnet restore "src/ApiGateway/DevBoard.ApiGateway/DevBoard.ApiGateway.csproj"

COPY src/ ./src/

WORKDIR /src/src/ApiGateway/DevBoard.ApiGateway
RUN dotnet build "DevBoard.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "DevBoard.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBoard.ApiGateway.dll"]