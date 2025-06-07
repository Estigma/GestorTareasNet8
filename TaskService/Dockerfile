FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

USER root

RUN apt-get update && \
    apt-get install -y curl wget iputils-ping && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskService/TaskService.csproj", "TaskService/"]
RUN dotnet restore "TaskService/TaskService.csproj"
COPY ["TaskService/", "TaskService/"]
WORKDIR "/src/TaskService"
RUN dotnet build "TaskService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["TaskService/init.sql", "TaskService/init-db.sh", "/usr/src/"]
RUN chmod +x /usr/src/init-db.sh

ENTRYPOINT ["dotnet", "TaskService.dll"]