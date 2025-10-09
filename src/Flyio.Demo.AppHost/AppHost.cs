var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Flyio_Demo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Flyio_Demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
