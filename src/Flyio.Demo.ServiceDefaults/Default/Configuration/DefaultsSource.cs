using Microsoft.Extensions.Configuration.Memory;

namespace Microsoft.Extensions.Configuration;

internal sealed class DefaultsSource : IConfigurationSource
{
    private readonly IConfiguration _configuration;

    public DefaultsSource(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var provider = new MemoryConfigurationProvider(new())
        {
            { "Authentication:Schemes:Bearer:ValidAudience", _configuration["OTEL_SERVICE_NAME"]  }
        };

        return provider;
    }
}
