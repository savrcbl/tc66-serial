using System.IO.Ports;
using System.Text;

namespace Tc66Serial;

/// <summary>
/// A client for reading measurements from an RDTech/FNIRSI TC66 or TC66C USB power meter
/// over a serial (virtual COM port) connection.
/// </summary>
/// <remarks>
/// This class is not thread-safe. Use one <see cref="Tc66Client"/> instance per device
/// and avoid calling its members concurrently from multiple threads.
/// </remarks>
public sealed class Tc66Client : IDisposable
{
    private readonly SerialPort _port;

    /// <summary>
    /// Creates a client for the given serial port. Call <see cref="Connect"/> before use.
    /// </summary>
    /// <param name="portName">The OS device name, e.g. "COM10" on Windows or "/dev/ttyACM0" on Linux.</param>
    /// <param name="baudRate">Baud rate to use. TC66 devices default to 115200.</param>
    /// <param name="timeoutMs">Read/write timeout, in milliseconds.</param>
    public Tc66Client(string portName, int baudRate = 115200, int timeoutMs = 3000)
    {
        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = timeoutMs,
            WriteTimeout = timeoutMs,
        };
    }

    /// <summary>The serial port name this client was constructed with.</summary>
    public string PortName => _port.PortName;

    /// <summary>True if the underlying serial port is currently open.</summary>
    public bool IsConnected => _port.IsOpen;

    /// <summary>Opens the underlying serial connection.</summary>
    public void Connect()
    {
        if (!_port.IsOpen)
            _port.Open();
    }

    /// <summary>Closes the underlying serial connection, if open.</summary>
    public void Disconnect()
    {
        if (_port.IsOpen)
            _port.Close();
    }

    /// <summary>Queries the device's current display mode.</summary>
    public string QueryMode()
    {
        EnsureConnected();
        byte[] buffer = SendCommandRaw("query", 4);
        return Encoding.ASCII.GetString(buffer).Trim('\0', ' ');
    }

    /// <summary>
    /// Requests, decrypts and parses a full measurement snapshot from the device.
    /// </summary>
    /// <exception cref="Tc66ProtocolException">Thrown if the response is malformed.</exception>
    public Tc66Reading GetReading()
    {
        EnsureConnected();
        byte[] raw = SendCommandRaw("getva", 192);
        byte[] plain = Tc66Codec.Decrypt(raw);
        return Tc66Codec.Parse(plain);
    }

    /// <summary>Returns the raw, still-encrypted 192-byte response from the device. Useful for debugging.</summary>
    public byte[] GetRawEncrypted()
    {
        EnsureConnected();
        return SendCommandRaw("getva", 192);
    }

    /// <summary>Navigates the device's on-screen display to the previous page.</summary>
    public void PreviousPage()
    {
        EnsureConnected();
        SendCommandRaw("lastp", 0);
    }

    /// <summary>Navigates the device's on-screen display to the next page.</summary>
    public void NextPage()
    {
        EnsureConnected();
        SendCommandRaw("nextp", 0);
    }

    /// <summary>Rotates the device's on-screen display.</summary>
    public void RotateScreen()
    {
        EnsureConnected();
        SendCommandRaw("rotat", 0);
    }

    /// <summary>Lists the serial ports currently visible to the operating system.</summary>
    public static string[] GetAvailablePorts() => SerialPort.GetPortNames();

    private byte[] SendCommandRaw(string command, int expectedLength)
    {
        _port.DiscardInBuffer();
        _port.DiscardOutBuffer();
        _port.Write(command);

        if (expectedLength <= 0)
            return Array.Empty<byte>();

        byte[] buffer = new byte[expectedLength];
        int totalRead = 0;
        while (totalRead < expectedLength)
        {
            int bytesRead = _port.Read(buffer, totalRead, expectedLength - totalRead);
            if (bytesRead == 0)
                break;
            totalRead += bytesRead;
        }

        if (totalRead < expectedLength)
        {
            throw new Tc66ProtocolException(
                $"Only received {totalRead}/{expectedLength} bytes in response to '{command}'.");
        }

        return buffer;
    }

    private void EnsureConnected()
    {
        if (!_port.IsOpen)
            throw new InvalidOperationException("Not connected. Call Connect() first.");
    }

    /// <summary>Disconnects and releases the underlying serial port.</summary>
    public void Dispose()
    {
        Disconnect();
        _port.Dispose();
    }
}
