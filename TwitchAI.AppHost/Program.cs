var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TwitchAI_Api>("twitch-ai-api");

builder.Build().Run();
