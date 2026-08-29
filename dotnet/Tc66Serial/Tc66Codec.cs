using System.Security.Cryptography;
using System.Text;

namespace Tc66Serial;

/// <summary>
/// Decrypts and parses the 192-byte response returned by a TC66 device's "getva" command.
/// </summary>
public static class Tc66Codec
{
    // Fixed AES-256 key used by TC66 / TC66C devices to encrypt their measurement packets.
    private static readonly byte[] AesKey =
    {
        0x58, 0x21, 0xfa, 0x56, 0x01, 0xb2, 0xf0, 0x26,
        0x87, 0xff, 0x12, 0x04, 0x62, 0x2a, 0x4f, 0xb0,
        0x86, 0xf4, 0x02, 0x60, 0x81, 0x6f, 0x9a, 0x0b,
        0xa7, 0xf1, 0x06, 0x61, 0x9a, 0xb8, 0x72, 0x88,
    };

    /// <summary>
    /// Decrypts a raw 192-byte response using AES-256-ECB (no padding).
    /// </summary>
    public static byte[] Decrypt(byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = AesKey;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
    }

    /// <summary>
    /// Parses a decrypted 192-byte buffer (three 64-byte packets: "pac1", "pac2", "pac3")
    /// into a <see cref="Tc66Reading"/>.
    /// </summary>
    /// <exception cref="Tc66ProtocolException">Thrown if the buffer is the wrong size or the packet headers are invalid.</exception>
    public static Tc66Reading Parse(byte[] plainText)
    {
        if (plainText.Length < 192)
            throw new Tc66ProtocolException($"Expected 192 decrypted bytes, got {plainText.Length}.");

        // Avoid the C# range operator here so this still compiles cleanly against
        // netstandard2.0, which has no built-in System.Range support.
        byte[] pac1 = new byte[64];
        byte[] pac2 = new byte[64];
        byte[] pac3 = new byte[64];
        Array.Copy(plainText, 0, pac1, 0, 64);
        Array.Copy(plainText, 64, pac2, 0, 64);
        Array.Copy(plainText, 128, pac3, 0, 64);

        string h1 = ReadAscii(pac1, 0, 4);
        string h2 = ReadAscii(pac2, 0, 4);
        string h3 = ReadAscii(pac3, 0, 4);

        if (h1 != "pac1" || h2 != "pac2" || h3 != "pac3")
        {
            throw new Tc66ProtocolException(
                $"Unexpected packet headers [{h1}] [{h2}] [{h3}]. " +
                "Decryption may have failed, or the response was truncated/corrupted.");
        }

        return new Tc66Reading
        {
            // pac1
            ProductName = ReadAscii(pac1, 4, 4),
            Version = ReadAscii(pac1, 8, 4),
            SerialNumber = ReadUInt32(pac1, 12),
            RunCount = ReadUInt32(pac1, 44),
            Voltage = ReadUInt32(pac1, 48) * 1e-4f,
            Current = ReadUInt32(pac1, 52) * 1e-5f,
            Power = ReadUInt32(pac1, 56) * 1e-4f,
            Pac1ChecksumValid = Crc16.ValidatePacket(pac1),

            // pac2
            Resistance = ReadUInt32(pac2, 4) * 1e-2f,
            Group0Mah = ReadUInt32(pac2, 8),
            Group0Mwh = ReadUInt32(pac2, 12),
            Group1Mah = ReadUInt32(pac2, 16),
            Group1Mwh = ReadUInt32(pac2, 20),
            TemperatureNegative = ReadUInt32(pac2, 24) == 1,
            Temperature = ReadUInt32(pac2, 28),
            DPlusVoltage = ReadUInt32(pac2, 32) * 1e-2f,
            DMinusVoltage = ReadUInt32(pac2, 36) * 1e-2f,
            Pac2ChecksumValid = Crc16.ValidatePacket(pac2),

            // pac3
            Pac3ChecksumValid = Crc16.ValidatePacket(pac3),
        };
    }

    private static string ReadAscii(byte[] data, int offset, int length) =>
        Encoding.ASCII.GetString(data, offset, length).Trim('\0', ' ');

    private static uint ReadUInt32(byte[] data, int offset) =>
        BitConverter.ToUInt32(data, offset);
}
