var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CleanArchBlazorWasm_Web>("web");

builder.Build().Run();
