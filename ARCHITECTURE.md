# Architecture

## Overview
This repository is a work in progress to offer a Twitch online service, with a primary focus on making an extensible chat bot.

In addition to applications and tests, it includes a set of supporting libraries to interact with Twitch, IGDB and a Twitch IRC over WebSocket chat client.

Working title has been BlibBloopBot just because it was an available username on Twitch, but that is very much a working title.

```
                 ┌────────────────────────────────────────────────┐
       ┌─────────┤                                                │
       │         │                 BlipBloopWeb                   │
┌──────▼─────┐   │                                                │
│ Clustering │   └────────────────────────────────────────────────┘
│  Storage   │
│  (Redis)   │   ┌─────────────────────────────────────────────────┐
└──────▲─────┘   │                                                 │
       │         │               BlipBloopService                  │  ┌─────────────────────────────────────────────────┐
       │         │                (Orleans Silo)                   │  │ Grain Persistent Stores (WIP: non persisted)    │
       │         │                                                 │  │                                                 │
       │         │   ┌───────────┐    ┌──────────────┐  ┌──────┐   │  │ ┌──────────────┬──────────────┬───────────────┐ │
       └─────────┤   │           │    │              ├─►│      │   ├──► │              │              │               │ │
                 │   │ UserGrain ├───►│ ChannelGrain │  │ Bot  │   │  │ │   Profile    │    Channel   │  BotSettings  │ │
                 │   │           │    │              │◄─┤      │   ◄──┤ │              │              │               │ │
                 │   └───────────┘    └──────────────┘  └──────┘   │  │ └──────────────┴──────────────┴───────────────┘ │
                 │                                                 │  │                                                 │
                 └─────────────────────────────────────────────────┘  └─────────────────────────────────────────────────┘

```

### Conceptoire.Twitch.Abstractions
Mostly interfaces, group the abstractions to work with Twitch.

### Conceptoire.Twitch
Implementation of the Twitch library.

The goal is to have it simple to use, declaring using a Fluent style API, while keeping it compatible with the framework dependency injection.

### BlipBloopBot: Standalone bot application
BlipBloopBot is a standalone .NET 5 command line application, mainly meant as a way to test and maintain abstractions around the primary bot service.

It can be used as a degraded, locally hosted version of the bot though.

### BlipBloopWeb: Web service
BlipBloopWeb is the asp.net core 5 application that acts as a user facing frontend to the Twitch service.

### BlipBloopService: Actor-hosting worker application
BlipBloopService is the core of the application, where bots are run and state is kept.

### BotServiceGrainInterface
Interface declaration of the hosted Orleans Grains (Actor types).

### BotServiceGrain
Implementation of the Orleans Grains (Actors).

### docker-compose
Simple docker-compose definition to run and debug the solution on a local Docker Desktop.

In Visual Studio this is the simplest way to get the service running.

### TwitchCategoriesCrawler
Quick and dirty crawler to enumerate and resolve Twitch categories using IGDB and Steam to get localized game synopsis.

Right now selected language is hardcoded, Twitch lets us enumerate up to 1000 categories.

Outputs a CSV file.
