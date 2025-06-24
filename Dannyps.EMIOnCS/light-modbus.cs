using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Dannyps.EMIOnCS;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EmiClock
{
    public ushort Year;
    public byte Month;
    public byte Day;
    public byte Weekday;
    public byte Hour;
    public byte Minute;
    public byte Second;
    public byte HundredthOfSecond;
    public ushort Deviation;
    public byte ClockStatus;
}

public class ModBus
{
    public interface Step1;

    public class ModBusBuilder : Step1
    {

        public const string SO_PATH = "../light-modbus/light-modbus.so";
        private readonly IntPtr _ctx;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModBusBuilder"/> class.
        /// </summary>
        /// <param name="device">The device path (e.g., "/dev/ttyUSB0").</param>
        /// <param name="baud">The baud rate (e.g, 9600).</param>
        /// <param name="parity">The parity setting (None, Even, Odd).</param>
        /// <param name="dataBit">The number of data bits (usually 8).</param>
        /// <param name="stopBit">The number of stop bits (usually 1 or 2).</param>
        /// <exception cref="Exception">Thrown if the modbus context cannot be created.</exception>
        /// <remarks>
        /// This constructor creates a new Modbus RTU context using the specified parameters.
        /// It initializes the context and checks for errors. If the context cannot be created,
        /// an exception is thrown. The underlying C library is expected to be available at the specified path.
        /// </remarks>
        /// <example>
        /// var modbus = new ModBus.ModBusBuilder("/dev/ttyUSB0", 9600, ModBus.Parity.None, 8, 2)
        ///     .SetSlave(1)
        ///     .SetDebug(false)
        ///     .SetErrorRecovery(ModbusErrorRecoveryMode.MODBUS_ERROR_RECOVERY_LINK |
        ///                       ModbusErrorRecoveryMode.MODBUS_ERROR_RECOVERY_PROTOCOL)
        ///     .SetResponseTimeout(TimeSpan.FromMilliseconds(200))
        ///     .Connect();
        /// </example>
        public ModBusBuilder(string device, int baud, Parity parity, int dataBit, int stopBit)
        {
            // Create a new modbus_t instance
            _ctx = modbus_new_rtu(device, baud, (byte)parity, dataBit, stopBit);
            if (_ctx == IntPtr.Zero)
            {
                throw new Exception("Failed to create modbus context");
            }
        }

        /// <summary>
        /// Sets the slave ID for the Modbus context.
        /// </summary>
        /// 
        /// <remarks>
        /// Unless changed, most EMI devices use slave ID 1.
        /// Slave ID is used to identify the specific device on the Modbus network.
        /// This implementation does not support multiple slaves on the same context.
        /// </remarks>
        public ModBusBuilder SetSlave(int slaveId)
        {
            // Set the slave ID for the modbus context
            var result = modbus_set_slave(_ctx, slaveId);
            if (result != 0)
            {
                throw new Exception($"Failed to set slave ID: {result}");
            }
            return this;
        }

        public ModBusBuilder SetDebug(bool enable)
        {
            // Enable or disable debug mode
            var result = modbus_set_debug(_ctx, enable ? 1 : 0);
            if (result != 0)
            {
                throw new Exception($"Failed to set debug mode: {result}");
            }
            return this;
        }

        public ModBusBuilder SetErrorRecovery(ModbusErrorRecoveryMode mode)
        {
            // Set the error recovery mode
            var result = modbus_set_error_recovery(_ctx, mode);
            if (result != 0)
            {
                throw new Exception($"Failed to set error recovery mode: {result}");
            }
            return this;
        }
        public ModBusBuilder SetResponseTimeout(TimeSpan timeSpan)
        {
            var miliseconds = (int)timeSpan.TotalMilliseconds;

            switch (miliseconds)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException(nameof(timeSpan), "Timeout must be non-negative");
                case 0:
                    // If timeout is zero, set it to a default value (e.g., 1000 ms)
                    miliseconds = 1000;
                    break;
                case > 60000:
                    throw new ArgumentOutOfRangeException(nameof(timeSpan), "Timeout must not exceed 60000 ms (1 minute)");
            }

            var result = modbus_set_response_timeout(_ctx, 0, (uint)((miliseconds % 1000) * 1000));

            if (result != 0)
            {
                throw new Exception($"Failed to set response timeout: {result}");
            }
            return this;
        }

        public ModBus Connect()
        {
            var result = modbus_connect(_ctx);
            if (result != 0)
            {
                throw new Exception($"Failed to connect: {result}");
            }
            return new ModBus(_ctx);
        }

        #region light-modbus.h External Methods

        // Define a pointer to the opaque modbus_t type
        [StructLayout(LayoutKind.Sequential)]
        public struct modbus_t { }

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr modbus_new_rtu([MarshalAs(UnmanagedType.LPStr)] string device, int baud, byte parity, int data_bit, int stop_bit);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int modbus_set_slave(IntPtr ctx, int slaveId);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int modbus_set_debug(IntPtr ctx, int flag);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int modbus_set_error_recovery(IntPtr ctx, ModbusErrorRecoveryMode mode);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int modbus_set_response_timeout(IntPtr ctx, uint to_sec, uint to_usec);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int modbus_connect(IntPtr ctx);

        [DllImport(SO_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr modbus_strerror(int errnum);
        #endregion

        public static string ModbusStrError(int errnum)
        {
            var ptr = modbus_strerror(errnum);
            return Marshal.PtrToStringAuto(ptr) ?? "Could not load error string."; // Convert const char* to C# string
        }

        internal IntPtr GetContext()
        {
            return _ctx;
        }

        internal static ModBus CreateFromConfiguration(ModBusConfiguration modBusConfig)
        {
            var builder = new ModBusBuilder(modBusConfig.Device, modBusConfig.BaudRate, modBusConfig.Parity, modBusConfig.DataBits, modBusConfig.StopBits)
                .SetSlave(modBusConfig.SlaveId)
                .SetDebug(modBusConfig.Debug)
                .SetErrorRecovery(ModbusErrorRecoveryMode.MODBUS_ERROR_RECOVERY_LINK | ModbusErrorRecoveryMode.MODBUS_ERROR_RECOVERY_PROTOCOL)
                .SetResponseTimeout(TimeSpan.FromMilliseconds(modBusConfig.ResponseTimeout));

            return builder.Connect();
        }
    }

    protected IntPtr _ctx;

    public enum Parity : ushort
    {
        None = 'N',
        Even = 'E',
        Odd = 'O'
    }

    private ModBus(IntPtr ctx)
    {
        _ctx = ctx;
    }

    public double GetDoubleFromUInt16(ushort registerAddress, sbyte scaler)
    {
        var result = getDoubleFromUInt16(_ctx, registerAddress, scaler, out var res);
        if (result != 1)
        {
            Console.WriteLine($"Error below");
            throw new Exception($"Failed to get double from UInt16: {ModBusBuilder.ModbusStrError(result)}.");
        }
        return res;
    }

    public double GetDoubleFromUInt32(ushort registerAddress)
    {
        var result = getDoubleFromUInt32(_ctx, registerAddress, out var res);
        if (result != 1)
        {
            Console.WriteLine($"Error below");
            throw new Exception($"Failed to get double from UInt32: {ModBusBuilder.ModbusStrError(result)}.");
        }
        return res;
    }

    public string GetOctetString(ushort registerAddress, int nb)
    {
        var ptr = getOctetString(_ctx, registerAddress, nb);
        if (ptr == IntPtr.Zero)
        {
            throw new Exception($"Failed to get octet string: {ModBusBuilder.ModbusStrError(Marshal.GetLastWin32Error())}");
        }
        try
        {
            return Marshal.PtrToStringAnsi(ptr) ?? string.Empty; // Convert const char* to C# string
        }
        finally
        {
            freeOctetString(ptr);
        }
    }

    public DateTime GetTime()
    {
        var ptr = getTime(_ctx);
        if (ptr == IntPtr.Zero)
        {
            throw new Exception($"Failed to get time: {ModBusBuilder.ModbusStrError(Marshal.GetLastWin32Error())}");
        }
        try
        {
            // time is represented as EmiClock struct
            var clock = Marshal.PtrToStructure<EmiClock>(ptr);
            // Convert EmiClock to DateTime
            return new DateTime(clock.Year, clock.Month, clock.Day, clock.Hour, clock.Minute, clock.Second, clock.HundredthOfSecond * 10, DateTimeKind.Utc);
        }
        finally
        {
            freeTime(ptr);
        }
    }

    #region emi-utils.h External Methods

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern int getDoubleFromUInt16(IntPtr ctx, ushort registerAddress, sbyte scaler, out double res);

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern int getDoubleFromUInt32(IntPtr ctx, ushort registerAddress, out double res);

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getOctetString(IntPtr ctx, ushort registerAddress, int nb);

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getTime(IntPtr ctx);

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern void freeTime(IntPtr time);

    [DllImport(ModBusBuilder.SO_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern void freeOctetString(IntPtr octetString);

    public class ModBusConfiguration
    {
        [Required]
        [StringLength(255, ErrorMessage = "Device path must not exceed 255 characters.")]
        [RegularExpression(@"^/dev/ttyUSB\d+$", ErrorMessage = "Device path must match the pattern /dev/ttyUSB[0-9]+.")]
        public string Device { get; set; } = null!;
        [Required]
        [Range(300, 115200, ErrorMessage = "Baud rate must be between 300 and 115200.")]
        public int BaudRate { get; set; }
        [Required]
        [EnumDataType(typeof(Parity), ErrorMessage = "Invalid parity value. Allowed values are None, Even, Odd.")]
        public Parity Parity { get; set; } = Parity.None;
        [Required]
        [Range(5, 8, ErrorMessage = "Data bits must be between 5 and 8.")]
        public int DataBits { get; set; }
        [Required]
        [Range(1, 2, ErrorMessage = "Stop bits must be either 1 or 2.")]
        public int StopBits { get; set; }
        [Required]
        [Range(1, 255, ErrorMessage = "Slave ID must be between 1 and 255.")]
        public int SlaveId { get; set; }
        
        public bool Debug { get; set; }
        
        /// <summary>
        /// The response timeout in milliseconds.
        /// </summary>
        public int ResponseTimeout { get; set; }
    }

    #endregion
}

public enum ModbusErrorRecoveryMode
{
    MODBUS_ERROR_RECOVERY_NONE = 0,
    MODBUS_ERROR_RECOVERY_LINK = 1 << 1,
    MODBUS_ERROR_RECOVERY_PROTOCOL = 1 << 2
};