namespace Tc66Serial;

/// <summary>
/// CRC-16/MODBUS implementation used to validate the 64-byte packets returned by TC66 devices.
/// </summary>
public static class Crc16
{
    /// <summary>
    /// Computes the CRC-16/MODBUS checksum over <paramref name="length"/> bytes of
    /// <paramref name="data"/>, starting at <paramref name="offset"/>.
    /// </summary>
    public static ushort ComputeModbus(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int bit = 0; bit < 8; bit++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }

    /// <summary>
    /// Validates a 64-byte TC66 packet. The CRC-16/MODBUS checksum is computed over
    /// the first 60 bytes and compared against the little-endian uint32 stored at offset 60
    /// (only the lower 16 bits of which are meaningful).
    /// </summary>
    public static bool ValidatePacket(byte[] packet)
    {
        if (packet.Length < 64)
            return false;

        uint storedCrc = BitConverter.ToUInt32(packet, 60);
        ushort computed = ComputeModbus(packet, 0, 60);
        return storedCrc == computed;
    }
}
