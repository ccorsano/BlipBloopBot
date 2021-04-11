# BlipBloopBot

[![.NET](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml)

## Twitch bot framework / service

> :warning: Work In Progress
>
> Name is very much WIP as well :sweat_smile:

### What is this ?

This is an evolving project to build a Twitch bot service in C# / dotnetcore.

The bot service is currently meant as a scalable hosted service:
 - ASP.NET Core 5 Web Service
 - Blazor Server UI (for fast prototyping, will reevaluate against a React/TypeScript frontend once featureset stabilized)
 - Orleans virtual actor backend for distribution
 - Containerized (currently not K8S for now)

Along the way, I am consolidating a C# Twitch .net library which is currently partially covering:
 - Helix API
 - IRC WS Chat Client
 - EventSub HTTP Subscriptions

## Configuration

To call into the Twitch and IGDB API, the different projects uses a Twitch app ClientId and Secret, that you can request on the [![Twitch dev console](https://dev.twitch.tv)](https://dev.twitch.tv).

> To run the Frontend service, you need to setup your app with a Redirect URI to https://localhost:5001/signin-oidc-fragment.
>
> If running through docker, you might have to add the same on the random port assigned by your Docker desktop.

Non-confidential settings can be set in the appsettings.json file.

Secrets are meant to be stored in secure storage.


### Secrets and credentials

Secrets and credentials are loaded as configuration.

For development, use the User Secrets feature as below or the Development appsetting.Development.json (but careful not to submit).

For deployment, Env variables will be loaded for configuration.

#### Setting required secrets for development

This is the crossplatform way, using the dotnet cli:
```
cd BlipBloopBot
dotnet user-secrets set "twitch:IrcOptions:OAuthToken" "<your_bot_user_token>"
cd BotWorkerService
dotnet user-secrets set "twitch:ClientSecret" "<your_twitch_app_secret>"
dotnet user-secrets set "twitch:IrcOptions:OAuthToken" "<your_bot_user_token>"
cd BlipBloopWeb
dotnet user-secrets set "twitch:ClientId" "<your_twitch_app_id>"
dotnet user-secrets set "twitch:ClientSecret" "<your_twitch_app_secret>"
dotnet user-secrets set "twitch:EventSub:WebHookSecret" "<any_random_string>"
```

## Getting started

The easiest way to debug and work is with Visual Studio, but it is not mandatory as the dotnet cli / VS Code is enough to have a good experience.

### Requirements
- .net core sdk 5: https://dotnet.microsoft.com/download/dotnet/5.0
- If launching the hosted service through docker: Docker Desktop, with WSL2 on Windows (because we run on Linux containers)

### Running the bot locally

To try to keep abstractions clean, in addition of the hosted service, the bot is also available as a standalone .net console application.

To run, launch the BlipBloopBot project in the solution.

- In Visual Studio, just set BlipBloopBot as startup project and hit F5 (or Debug).
Take a look at the configuration section above, as you may want to right-clic and "Manage User Secrets" first.
- If not using Visual Studio. First look at the configuration section above and run

```
cd BlipBloopBot
dotnet run
```

### Running the hosted service with auto-reload to work on UI

When working on the FrontEnd, the easiest is to run the frontend and backend in watch mode and attach the debugger from Visual Studio as required.

First, look at the configuration section above to setup secrets.

Then open one terminal window for the worker:

```
cd BotWorkerService
dotnet watch
```

And another one for the Frontend / web server:

```
cd BlipBloopWeb
dotnet watch
```

### Running the hosted service closer to prod through docker compose

In Visual Studio, set the docker-compose project as startup and hit Debug.

The docker-hosted service is also reading the User Secrets files.


# Generating IGDB API Types

The IGDB client uses their published Protobuf format, which will need to be updated when IGDB update their interfaces.

Currently this needs to be done manually.

Download file and generate classes

```
dotnet tool install --global protobuf-net.Protogen --version 3.0.73
cd Conceptoire.Twitch\IGDB
(wget https://api.igdb.com/v4/igdbapi.proto).Content -replace "import ""google/protobuf/timestamp.proto"";","import ""google/protobuf/timestamp.proto""; option csharp_namespace = ""Conceptoire.Twitch.IGDB.Generated"";" | Out-File -FilePath .\igdbapi.proto -Encoding utf8 -Force
protogen --csharp_out=Generated .\igdbapi.proto
```
