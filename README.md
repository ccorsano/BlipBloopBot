# BlipBloopBot

[![.NET](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml)

## Twitch bot framework / service

> :warning: Work In Progress
> Name is very much WIP as well :sweat_smile:

# Configuration

To call into the Twitch and IGDB API, the different projects uses a Twitch app ClientId and Secret, that you can request on the [![Twitch dev console](https://dev.twitch.tv)](https://dev.twitch.tv).

Non-confidential settings can be set in the appsettings.json file.

Secrets are meant to be stored in secure storage.


## Secrets and credentials

Secrets and credentials are loaded as configuration.

For development, use the User Secrets feature as below or the Development appsetting.Development.json (but careful not to submit).

For deployment, Env variables will be loaded for configuration.

### Setting required secrets for development

This is the crossplatform way, using the dotnet cli:
```
cd TwitchAchievementTrackerBackend/TwitchAchievementTrackerBackend
dotnet user-secrets set "twitch:ClientSecret" "<your_twitch_app_secret>"
dotnet user-secrets set "twitch:IrcOptions:OAuthToken" "<your_bot_user_token>"
```

# Generating IGDB API Types

The IGDB client uses their published Protobuf format, which will need to be updated when IGDB update their interfaces.

Currently this needs to be done manually.

Download file and generate classes

``
dotnet tool install --global protobuf-net.Protogen --version 3.0.73
cd BlipBloopFramework\IGDB
(wget https://api.igdb.com/v4/igdbapi.proto).Content -replace "import ""google/protobuf/timestamp.proto"";","import ""google/protobuf/timestamp.proto""; option csharp_namespace = ""BlibBloopBot.IGDB.Generated"";" | Out-File -FilePath .\igdbapi.proto -Encoding utf8 -Force
protogen --csharp_out=Generated .\igdbapi.proto
``
