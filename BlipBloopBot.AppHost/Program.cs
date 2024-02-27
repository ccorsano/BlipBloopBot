var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlipBloopWeb>("blipbloopweb");
builder.AddProject<Projects.BotWorkerService>("botworkerservice");

builder.Build().Run();
