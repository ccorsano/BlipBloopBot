# BlipBloopBot

[![.NET](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ccorsano/BlipBloopBot/actions/workflows/dotnet.yml)

WIP Twitch bot framework / service
Name is very much WIP as well :D

# Generate IGDB API Types

Download file and generate classes

``
dotnet tool install --global protobuf-net.Protogen --version 3.0.73
cd BlipBloopFramework\IGDB
(wget https://api.igdb.com/v4/igdbapi.proto).Content -replace "import ""google/protobuf/timestamp.proto"";","import ""google/protobuf/timestamp.proto""; option csharp_namespace = ""BlibBloopBot.IGDB.Generated"";" | Out-File -FilePath .\igdbapi.proto -Encoding utf8 -Force
protogen --csharp_out=Generated .\igdbapi.proto
``
