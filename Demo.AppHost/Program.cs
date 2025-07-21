var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.API_IA>("api-ia");

builder.AddAzureFunctionsProject<Projects.FX_AI>("fx-ai");

builder.Build().Run();
