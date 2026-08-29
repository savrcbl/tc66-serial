// Minimal example: connect, take one reading, print it.
//
// Run with:
//   dotnet run --project examples/dotnet-cli -- COM10
// (or /dev/ttyACM0 on Linux/macOS)

using Tc66Serial;

if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run -- <port> [baudRate]");
    return 1;
}

string port = args[0];
int baudRate = args.Length > 1 ? int.Parse(args[1]) : 115200;

using var client = new Tc66Client(port, baudRate);
client.Connect();
Console.WriteLine($"Connected to {port} at {baudRate} baud.");

Tc66Reading reading = client.GetReading();

Console.WriteLine($"Device        : {reading.ProductName} (firmware {reading.Version}, serial {reading.SerialNumber})");
Console.WriteLine($"Voltage       : {reading.Voltage:F4} V");
Console.WriteLine($"Current       : {reading.Current:F5} A");
Console.WriteLine($"Power         : {reading.Power:F4} W");
Console.WriteLine($"Resistance    : {reading.Resistance:F2} Ohm");
Console.WriteLine($"Group 0       : {reading.Group0Mah} mAh / {reading.Group0Mwh} mWh");
Console.WriteLine($"Group 1       : {reading.Group1Mah} mAh / {reading.Group1Mwh} mWh");
Console.WriteLine($"Temperature   : {reading.SignedTemperature}");
Console.WriteLine($"D+/D-         : {reading.DPlusVoltage:F2} V / {reading.DMinusVoltage:F2} V");
Console.WriteLine($"Checksums OK  : {reading.IsValid}");

return 0;
