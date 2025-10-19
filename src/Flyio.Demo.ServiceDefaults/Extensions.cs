using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using Steeltoe.Configuration.ConfigServer;
using Steeltoe.Configuration.Placeholder;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureRemoteConfiguration();

        builder.Configuration.AddPlaceholderResolver();

        return builder;
    }

    public static TBuilder ConfigureRemoteConfiguration<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var shouldUse = !string.IsNullOrWhiteSpace(builder.Configuration["spring:cloud:config:uri"]);

        if (shouldUse)
        {
            var otelServiceName = builder.Configuration["OTEL_SERVICE_NAME"];

            var memorySource = new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?>
                {
                    // Setup defaults for Spring Cloud Config
                    { "spring:application:name", otelServiceName },
                    // { "spring:cloud:config:uri", "http://pla-config-server.config:8888" },
                    { "spring:cloud:config:failFast", "true" },
                }
            };

            builder.Configuration.Sources.Insert(0, memorySource);

            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });

            builder.AddConfigServer(loggerFactory);
        }

        return builder;
    }
}
