var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.YourGameServer_Game>("Game");

builder.AddProject<Projects.YourGameServer_Explorer>("Explorer")
    .WithExternalHttpEndpoints();

builder.Build().Run();
