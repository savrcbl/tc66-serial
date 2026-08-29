namespace Tc66Serial;

/// <summary>
/// Thrown when a TC66 device returns a response that cannot be decrypted or parsed,
/// or that fails its CRC-16/MODBUS checksum.
/// </summary>
public sealed class Tc66ProtocolException : Exception
{
    /// <summary>Creates a new <see cref="Tc66ProtocolException"/> with the given message.</summary>
    public Tc66ProtocolException(string message) : base(message) { }

    /// <summary>Creates a new <see cref="Tc66ProtocolException"/> with the given message and inner exception.</summary>
    public Tc66ProtocolException(string message, Exception innerException) : base(message, innerException) { }
}
