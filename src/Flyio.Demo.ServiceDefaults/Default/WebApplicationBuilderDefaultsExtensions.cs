using Flyio.Demo.ServiceDefaults;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationBuilderDefaultsExtensions
{
    public static WebApplicationBuilder AddDefaults(this WebApplicationBuilder builder)
    {
        // builder.Configuration.AddDefaults(builder.Configuration);

        // var memorySource = new MemoryConfigurationSource
        // {
        //     InitialData = new Dictionary<string, string?>
        //         {
        //             { "xApplicationName", builder.Configuration["OTEL_SERVICE_NAME"] },
        //         }
        // };

        builder.Configuration.AddInMemoryCollection(new MemoryConfigurationProvider(new())
        {
            { "Authentication:Schemes:Bearer:ValidAudience", builder.Configuration["OTEL_SERVICE_NAME"]  }
        });

        builder.Services.AddHttpContextAccessor();

        builder.AddDefaultHealthChecks();

        builder.AddHttpLoggingDefaults();

        builder.AddOpenTelemetryDefaults();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            // http.AddStandardResilienceHandler();

            http.ConfigureHttpClient(configuration => configuration.DefaultRequestHeaders.UserAgent.ParseAdd(builder.Configuration["ApplicationName"]));

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        // Remove server:kestrel response header
        builder.Services.Configure<KestrelServerOptions>(kestrelServerOptions =>
        {
            kestrelServerOptions.AddServerHeader = false;
        });

        return builder;
    }

    public static WebApplicationBuilder AddHttpLoggingDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.Duration;
            logging.CombineLogs = true;
        });

        builder.Services.AddHttpLoggingInterceptor<FilterRequestLoggingInterceptor>();

        return builder;
    }

    public static WebApplicationBuilder AddOpenTelemetryDefaults(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    ;
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                    {
                        tracing.RecordException = true;

                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(WebApplicationDefaultsExtensions.HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(WebApplicationDefaultsExtensions.AlivenessEndpointPath);
                    })
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    // .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static WebApplicationBuilder AddOpenTelemetryExporters(this WebApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static WebApplicationBuilder AddDefaultHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }
}