var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.TwitchAI_Api>("twitch-ai-api");

// Добавляем SPA (Vite/Node) как NPM‑приложение
var web = builder.AddNpmApp("twitch-ai-web", "../TwitchAI.Web")
	.WithReference(api)
	.WithEnvironment("VITE_API_BASE_URL", "http://localhost:5212/api")
	.WithHttpEndpoint(targetPort: 5173)
	.WithExternalHttpEndpoints();

builder.Build().Run();
