namespace Dannyps.EMIOnCS;

public class Program
{
    public static void Main(string[] args)
    {
        // Load settings from appsettings.json
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = configurationBuilder.Build();

        var (modBusConfig, _) = configuration.ConfigureSection<ModBus.ModBusConfiguration>();

        var modbus = ModBus.ModBusBuilder.CreateFromConfiguration(modBusConfig);

        var value = modbus.GetTime();

        Console.WriteLine($"Time from EMI device: {value}");
    }
}
