using Dannyps.EMIOnCS.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dannyps.EMIOnCS;

public class Program
{
    public static void Main(string[] _args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

        ServiceProvider sp = ConfigureServices(configuration);

        var appConfig = sp.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;

        var modbus = ModBus.ModBusBuilder.CreateFromConfiguration(appConfig.ModBusConfiguration);
        

    }

    private static ServiceProvider ConfigureServices(IConfigurationRoot configuration)
    {
        var services = new ServiceCollection();

        services.ConfigureAndValidate<ApplicationConfiguration>(configuration.Bind);

        return services.BuildServiceProvider();
    }
}
