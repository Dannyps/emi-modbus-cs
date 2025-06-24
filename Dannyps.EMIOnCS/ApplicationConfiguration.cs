using System.ComponentModel.DataAnnotations;

namespace Dannyps.EMIOnCS;

public class ApplicationConfiguration
{
    public ModBus.ModBusConfiguration ModBusConfiguration { get; set; } = null!;
    public EmiConfig EmiConfig { get; set; } = new EmiConfig();
}

public class EmiConfig
{
    public EmiDataLoad[] DataLoads { get; set; } = [];
    public MQTTConfiguration MqttConfiguration { get; set; } = new MQTTConfiguration();
    public class EmiDataLoad
    {
        /// <summary>
        /// The name of the data load, which is used to identify it in the system.
        /// It should be unique within the context of the application.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// The address of the data load in the Modbus register map.
        /// </summary>
        [RegularExpression(@"^(0x)?[0-9A-Fa-f]+$", ErrorMessage = "Address must be a valid hexadecimal number.")]
        [Required]
        public string Address { get; set; } = string.Empty;

        public ushort GetAddressAsUShort()
        {
            if (ushort.TryParse(Address.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var addressValue))
            {
                return addressValue;
            }
            throw new FormatException($"Address {Address} is not a valid hexadecimal number.");
        }

        /// <summary>
        /// The type of data load, which can be Float, Double, String, or Clock.
        /// </summary>
        [Required]
        [EnumDataType(typeof(DataLoadType), ErrorMessage = "Invalid data load type.")]
        public DataLoadType Type { get; set; } = DataLoadType.Unset;

        /// <summary>
        /// The Unit of measurement for this data load.
        /// Not all data loads admit units, so this property may not be applicable for all data loads.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// The Scaler is related to the resolution of the received data, i.e.,
        /// to determine how many decimal digits should be considered.
        /// For example, with a scaler of -2, the received data 98 corresponds to the value 0,98.
        /// If the scaler is 0, the received data is considered as is.
        /// Not all data types support scalers, so this property may not be applicable for all data loads.
        /// </summary>
        public int Scaler { get; set; } = 0;

        /// <summary>
        /// The length of the string data load.
        /// This is only applicable for String data types.
        /// </summary>
        public ushort StringLength { get; set; } = 0;

        public sbyte GetScalerAsSByte()
        {
            if (sbyte.TryParse(Scaler.ToString(), out var scalerValue))
            {
                return scalerValue;
            }
            throw new FormatException("Scaler is not a valid sbyte number.");
        }

        /// <summary>
        /// The name of the MQTT topic to publish this data load.
        /// </summary>
        [Required]
        public string MQTTTopic { get; set; } = string.Empty;
        /// <summary>
        /// Polling interval in seconds for this data load.
        /// </summary>
        [Required]
        public int PollingInterval { get; set; } = 1; // Default to 1 second

    }

    public class MQTTConfiguration
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public enum DataLoadType
    {
        Float,
        Double,
        Unsigned,
        String,
        Clock,
        Unset
    }
}
