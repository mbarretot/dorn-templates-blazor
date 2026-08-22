var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CleanArchBlazorServer_Web>("web");

builder.Build().Run();
