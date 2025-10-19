namespace Microsoft.Extensions.Configuration;

internal static class DefaultsConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddDefaults(this IConfigurationBuilder builder, IConfiguration configuration)
    {
        return builder.Add(new DefaultsSource(configuration));
    }
}
