#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BlipBloopWeb/BlipBloopWeb.csproj", "BlipBloopWeb/"]
COPY ["BotServiceGrainInterface/BotServiceGrainInterface.csproj", "BotServiceGrainInterface/"]
COPY ["Conceptoire.Twitch/Conceptoire.Twitch.csproj", "Conceptoire.Twitch/"]
COPY ["Conceptoire.Twitch.Abstractions/Conceptoire.Twitch.Abstractions.csproj", "Conceptoire.Twitch.Abstractions/"]
RUN dotnet restore "BlipBloopWeb/BlipBloopWeb.csproj"
COPY . .
WORKDIR "/src/BlipBloopWeb"
RUN dotnet build "BlipBloopWeb.csproj" -c Release -o /app/build -p:ContinuousIntegrationBuild=true

FROM build AS publish
RUN dotnet publish "BlipBloopWeb.csproj" -c Release -o /app/publish -p:ContinuousIntegrationBuild=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# WebHook secret used for Twitch EventSub webhook registration & security
ENV Twitch__EventSub__WebHookSecret=""
# Redis endpoint for clustering
ENV REDIS_URL "redis://redis:6379"
ENTRYPOINT ["dotnet", "BlipBloopWeb.dll"]
