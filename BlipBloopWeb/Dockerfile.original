#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["BlipBloopWeb/BlipBloopWeb.csproj", "BlipBloopWeb/"]
RUN dotnet restore "BlipBloopWeb/BlipBloopWeb.csproj"
COPY . .
WORKDIR "/src/BlipBloopWeb"
RUN dotnet build "BlipBloopWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlipBloopWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# WebHook secret used for Twitch EventSub webhook registration & security
ENV Twitch__EventSub__WebHookSecret=""

ENTRYPOINT ["dotnet", "BlipBloopWeb.dll"]