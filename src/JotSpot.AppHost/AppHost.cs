var builder = DistributedApplication.CreateBuilder(args);

var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.JotSpot_Api>("jots-api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(identityApi)
    .WaitFor(identityApi);

builder.Build().Run();
