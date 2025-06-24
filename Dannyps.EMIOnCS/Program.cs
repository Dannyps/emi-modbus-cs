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

        var pollingTasks = appConfig.EmiConfig.DataLoads
            .Select(dataLoad => new PollingTask
            {
                Config = dataLoad,
                NextRun = DateTime.UtcNow
            })
            .ToList();

        while (true)
        {
            foreach (var task in pollingTasks.Where(t => t.NextRun.HasValue && t.NextRun.Value <= DateTime.UtcNow))
            {
                try
                {
                    var data = "";
                    switch (task.Config.Type)
                    {
                        case EmiConfig.DataLoadType.Float:
                            var floatData = modbus.GetDoubleFromUInt16(task.Config.GetAddressAsUShort(), task.Config.GetScalerAsSByte());
                            Console.WriteLine($"Float data for {task.Config.Name}: {floatData} {task.Config.Unit}");
                            data = floatData.ToString();
                            break;

                        case EmiConfig.DataLoadType.Double:
                            var doubleData = modbus.GetDoubleFromUInt32(task.Config.GetAddressAsUShort(), task.Config.GetScalerAsSByte());
                            Console.WriteLine($"Double data for {task.Config.Name}: {doubleData} {task.Config.Unit}");
                            data = doubleData.ToString();
                            break;

                        case EmiConfig.DataLoadType.String:
                            var stringData = modbus.GetOctetString(task.Config.GetAddressAsUShort(), task.Config.StringLength);
                            Console.WriteLine($"String data for {task.Config.Name}: {stringData}");
                            data = stringData;
                            break;

                        case EmiConfig.DataLoadType.Clock:
                            var clockData = modbus.GetTime();
                            Console.WriteLine($"Clock data for {task.Config.Name}: {clockData}");
                            data = clockData.ToString("o"); // ISO 8601 format
                            break;

                        case EmiConfig.DataLoadType.Unsigned:
                            var unsignedData = modbus.GetUnsigned(task.Config.GetAddressAsUShort());
                            Console.WriteLine($"Unsigned data for {task.Config.Name}: {unsignedData} {task.Config.Unit}");
                            data = unsignedData.ToString();
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported data type: {task.Config.Type}");
                    }
                    Console.WriteLine($"Data for {task.Config.Name}: {data}");
                    // Here you would publish the data to MQTT using the configured topic.
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading data for {task.Config.Name}: {ex.Message}");
                }

                // Schedule the next run
                task.NextRun = DateTime.UtcNow.AddSeconds(task.Config.PollingInterval);

            }

            // Sleep for a short duration to avoid busy waiting
            Thread.Sleep(1000);
        }

    }

    private static ServiceProvider ConfigureServices(IConfigurationRoot configuration)
    {
        var services = new ServiceCollection();

        services.ConfigureAndValidate<ApplicationConfiguration>(configuration.Bind);

        return services.BuildServiceProvider();
    }
}

class PollingTask
{
    public DateTime? NextRun { get; set; }
    public required EmiConfig.EmiDataLoad Config { get; set; }
}
