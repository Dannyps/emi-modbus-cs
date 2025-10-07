# GitHub Copilot Instructions for emi-modbus-cs

## Project Overview

This is a C# application that interfaces with EMI (Energy Measurement Interface) devices using the Modbus RTU protocol over serial communication. The application reads data from Modbus registers and publishes the data to an MQTT broker for monitoring and integration purposes.

## Architecture

### Components

1. **C# Application** (`Dannyps.EMIOnCS/`): Main .NET 8.0 console application
2. **Native Modbus Library** (`light-modbus/`): C library providing Modbus RTU communication functionality
3. **P/Invoke Wrapper** (`light-modbus.cs`): C# wrapper for the native C library using DllImport

### Key Files

- `Program.cs`: Main entry point with polling loop for reading Modbus data
- `light-modbus.cs`: C# wrapper for native Modbus library functions
- `ApplicationConfiguration.cs`: Configuration classes for application settings
- `appsettings.json`: Configuration file (not in source control, use `appsettings.example.json` as template)
- `light-modbus/`: Native C library implementing Modbus RTU and EMI utilities

## Coding Standards

### C# Code

- Use .NET 8.0 features and modern C# patterns
- Follow nullable reference types conventions (enabled in project)
- Use data annotations for configuration validation (`[Required]`, `[RegularExpression]`, etc.)
- Prefer explicit typing over `var` for public APIs and interfaces
- Use async/await patterns for I/O operations (MQTT, file operations)

### Native Interop

- All P/Invoke declarations should use `[DllImport(ModBusBuilder.SO_PATH)]`
- Properly marshal strings using `[MarshalAs(UnmanagedType.LPStr)]`
- Handle memory cleanup for native pointers (see `freeTime`, `freeOctetString`)
- The native library path is `"../light-modbus/light-modbus.so"`

### Configuration

- All configuration classes should have data validation attributes
- Use `ConfigureAndValidate<T>` extension for configuration binding with validation
- Support hexadecimal addresses with optional "0x" prefix (e.g., "0x006c")

## Data Types

### Modbus Data Load Types

The application supports reading these data types from Modbus registers:

- `Float`: 16-bit unsigned integer with scaler (UInt16)
- `Double`: 32-bit unsigned integer with scaler (UInt32)
- `Unsigned`: 8-bit unsigned integer
- `String`: Octet string with specified length
- `Clock`: EMI clock structure

### Scaling

Data can be scaled using a `Scaler` property (signed byte):
- Positive scaler: multiply by 10^scaler
- Negative scaler: divide by 10^|scaler|
- Zero scaler: no scaling

## Building and Testing

### Build Commands

```bash
# Build C# application
dotnet build

# Build native library
cd light-modbus
make
```

### Native Library

The C library must be built before running the application:
- Produces `light-modbus.so` in the `light-modbus/` directory
- Requires C compiler and standard build tools
- Uses `-O2 -Wall -Wpedantic -fPIC` flags

## Common Patterns

### Adding New Data Load Types

1. Add enum value to `DataLoadType` in `ApplicationConfiguration.cs`
2. Add corresponding case in `ProcessTasks` method in `Program.cs`
3. Add native function declaration if needed in `light-modbus.cs`
4. Add wrapper method in `ModBus` class

### Configuration Validation

Use data annotations for validation:
- `[Required]` for mandatory fields
- `[RegularExpression]` for format validation
- `[Range]` for numeric bounds
- `[StringLength]` for string length constraints

### MQTT Integration

- MQTT client is created per polling cycle
- Messages are published synchronously during task processing
- Connection uses credentials from configuration
- Client is properly disposed after each cycle

## Important Notes

### Serial Communication

- Device paths follow pattern `/dev/ttyUSB[0-9]+`
- Supported baud rates: 300-115200
- Parity options: None, Even, Odd
- Default settings often: 9600 baud, 8 data bits, 2 stop bits

### Error Handling

- Native library errors are converted using `modbus_strerror`
- Configuration validation errors are caught during startup
- Modbus communication errors should be logged but not stop the polling loop

### Memory Management

- Native pointers must be freed using appropriate `free*` functions
- Use `using` statements for MQTT client disposal
- IntPtr contexts are managed by ModBusBuilder

## Dependencies

### NuGet Packages

- `Microsoft.Extensions.Configuration.Json`: Configuration management
- `Microsoft.Extensions.DependencyInjection`: DI container
- `MQTTnet`: MQTT client library
- `ReHackt.Extensions.Options.Validation`: Configuration validation

### System Requirements

- .NET 8.0 SDK
- C compiler (gcc/clang) for building native library
- Serial port access for Modbus communication
- MQTT broker for publishing data

## Testing

Currently, this project does not have automated tests. When adding tests:
- Consider creating a test project following .NET conventions
- Mock serial communication for unit tests
- Use integration tests for end-to-end Modbus communication scenarios
- Test configuration validation thoroughly
