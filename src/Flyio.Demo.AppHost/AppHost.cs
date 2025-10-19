var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", "admin");
var password = builder.AddParameter("password", "admin");

var otelUrl = builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"];

var keycloak = builder
    .AddKeycloak("keycloak", 8080, adminPassword: password)
    .WithRealmImport("./keycloak/realms")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment(context =>
    {
        // context.EnvironmentVariables["KC_TRACING_ENABLED"] = "true";
        // context.EnvironmentVariables["KC_TRACING_ENDPOINT"] = new HostUrl(otelUrl!);
    })
    .WithDataVolume()
    ;

var apiService = builder.AddProject<Projects.Flyio_Demo_ApiService>("apiservice")
    .WaitFor(keycloak)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Flyio_Demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
