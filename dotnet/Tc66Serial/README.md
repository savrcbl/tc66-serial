# Tc66Serial

A .NET library for reading live measurements from RDTech/FNIRSI **TC66** and **TC66C**
USB power meters over a serial connection: voltage, current, power, resistance, D+/D-
line voltage, and the two mAh/mWh accumulator groups.

```csharp
using Tc66Serial;

using var client = new Tc66Client("COM10");
client.Connect();

Tc66Reading reading = client.GetReading();
Console.WriteLine($"{reading.Voltage:F4} V  {reading.Current:F5} A  {reading.Power:F4} W");
```

Full documentation, protocol notes, and the companion npm package live in the
[GitHub repository](https://github.com/savrcbl/tc66-serial).
