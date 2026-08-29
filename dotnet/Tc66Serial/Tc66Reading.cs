namespace Tc66Serial;

/// <summary>
/// A single decoded measurement snapshot read from a TC66 / TC66C device.
/// </summary>
public sealed class Tc66Reading
{
    /// <summary>Product name reported by the device (e.g. "TC66").</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>Firmware version string reported by the device (e.g. "1.18").</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Device serial number.</summary>
    public uint SerialNumber { get; init; }

    /// <summary>Number of measurement runs recorded by the device.</summary>
    public uint RunCount { get; init; }

    /// <summary>Bus voltage, in volts.</summary>
    public float Voltage { get; init; }

    /// <summary>Bus current, in amperes.</summary>
    public float Current { get; init; }

    /// <summary>Bus power, in watts.</summary>
    public float Power { get; init; }

    /// <summary>Estimated resistance of the load, in ohms.</summary>
    public float Resistance { get; init; }

    /// <summary>Accumulated capacity for measurement group 0, in milliamp-hours.</summary>
    public float Group0Mah { get; init; }

    /// <summary>Accumulated energy for measurement group 0, in milliwatt-hours.</summary>
    public float Group0Mwh { get; init; }

    /// <summary>Accumulated capacity for measurement group 1, in milliamp-hours.</summary>
    public float Group1Mah { get; init; }

    /// <summary>Accumulated energy for measurement group 1, in milliwatt-hours.</summary>
    public float Group1Mwh { get; init; }

    /// <summary>True if the reported temperature is negative.</summary>
    public bool TemperatureNegative { get; init; }

    /// <summary>Unsigned temperature magnitude reported by the device. Units (C or F) depend on the device's display setting.</summary>
    public uint Temperature { get; init; }

    /// <summary>Signed temperature, combining <see cref="TemperatureNegative"/> and <see cref="Temperature"/>.</summary>
    public int SignedTemperature => TemperatureNegative ? -(int)Temperature : (int)Temperature;

    /// <summary>USB D+ line voltage, in volts.</summary>
    public float DPlusVoltage { get; init; }

    /// <summary>USB D- line voltage, in volts.</summary>
    public float DMinusVoltage { get; init; }

    /// <summary>True if the first 64-byte packet passed its CRC-16/MODBUS check.</summary>
    public bool Pac1ChecksumValid { get; init; }

    /// <summary>True if the second 64-byte packet passed its CRC-16/MODBUS check.</summary>
    public bool Pac2ChecksumValid { get; init; }

    /// <summary>True if the third 64-byte packet passed its CRC-16/MODBUS check.</summary>
    public bool Pac3ChecksumValid { get; init; }

    /// <summary>True if all three packets passed their CRC-16/MODBUS checks.</summary>
    public bool IsValid => Pac1ChecksumValid && Pac2ChecksumValid && Pac3ChecksumValid;
}
