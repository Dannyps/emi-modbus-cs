using Microsoft.Extensions.Configuration;

namespace Dannyps.EMIOnCS.Extensions;

public static class ConfigurationExtensions
{
    public static (T, IConfigurationSection) ConfigureSection<T>(this IConfiguration configuration, string? sectionName = null) where T : class, new()
    {
        var localConfiguration = new T();
        var configSection = configuration.GetSection(sectionName ?? typeof(T).Name);
        configSection.Bind(localConfiguration);
        return (localConfiguration, configSection);
    }
}