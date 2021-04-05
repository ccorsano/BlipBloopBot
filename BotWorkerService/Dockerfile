#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["BotWorkerService/BotWorkerService.csproj", "BotWorkerService/"]
COPY ["BlipBloopCommands/BlipBloopCommands.csproj", "BlipBloopCommands/"]
COPY ["BotServiceGrain/BotServiceGrain.csproj", "BotServiceGrain/"]
COPY ["BotServiceGrainInterface/BotServiceGrainInterface.csproj", "BotServiceGrainInterface/"]
COPY ["Conceptoire.Twitch/Conceptoire.Twitch.csproj", "Conceptoire.Twitch/"]
COPY ["Conceptoire.Twitch.Abstractions/Conceptoire.Twitch.Abstractions.csproj", "Conceptoire.Twitch.Abstractions/"]
RUN dotnet restore "BotWorkerService/BotWorkerService.csproj"
COPY . .
WORKDIR "/src/BotWorkerService"
RUN dotnet build "BotWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BotWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV HOSTNAME "localhost"
ENV REDIS_URL "redis://redis:6379"
EXPOSE 11111
EXPOSE 30000
ENTRYPOINT ["dotnet", "BotWorkerService.dll"]