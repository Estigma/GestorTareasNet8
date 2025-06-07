FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClientService/ClientService.csproj", "ClientService/"]
RUN dotnet restore "ClientService/ClientService.csproj"
COPY ["ClientService/", "ClientService/"]
WORKDIR "/src/ClientService"
RUN dotnet build "ClientService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClientService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["ClientService/init.sql", "ClientService/init-db.sh", "/usr/src/"]
RUN chmod +x /usr/src/init-db.sh

ENTRYPOINT ["dotnet", "ClientService.dll"]