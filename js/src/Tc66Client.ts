import { SerialPort } from 'serialport';
import { decrypt, parse } from './codec.js';
import { Tc66ProtocolError, type Tc66ClientOptions, type Tc66Reading } from './types.js';

/**
 * A client for reading measurements from an RDTech/FNIRSI TC66 or TC66C USB power meter
 * over a serial connection.
 *
 * Not safe to use concurrently from multiple call sites at once — each call writes a
 * command and waits for its response before the next one should be issued.
 */
export class Tc66Client {
  private readonly port: SerialPort;
  private readonly timeoutMs: number;

  /**
   * Creates a client for the given serial port. Call {@link connect} before use.
   *
   * @param path The OS device path, e.g. "COM10" on Windows or "/dev/ttyACM0" on Linux/macOS.
   * @param options Optional baud rate / timeout overrides.
   */
  constructor(path: string, options: Tc66ClientOptions = {}) {
    this.timeoutMs = options.timeoutMs ?? 3000;
    this.port = new SerialPort({
      path,
      baudRate: options.baudRate ?? 115200,
      dataBits: 8,
      parity: 'none',
      stopBits: 1,
      autoOpen: false,
    });
  }

  /** True if the underlying serial port is currently open. */
  get isConnected(): boolean {
    return this.port.isOpen;
  }

  /** Opens the underlying serial connection. */
  connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.port.open((err) => (err ? reject(err) : resolve()));
    });
  }

  /** Closes the underlying serial connection, if open. */
  disconnect(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (!this.port.isOpen) {
        resolve();
        return;
      }
      this.port.close((err) => (err ? reject(err) : resolve()));
    });
  }

  /** Queries the device's current display mode. */
  async queryMode(): Promise<string> {
    const buf = await this.sendCommand('query', 4);
    return buf.toString('ascii').replace(/\0/g, '').trim();
  }

  /**
   * Requests, decrypts and parses a full measurement snapshot from the device.
   *
   * @throws {Tc66ProtocolError} If the response is malformed.
   */
  async getReading(): Promise<Tc66Reading> {
    const raw = await this.sendCommand('getva', 192);
    const plain = decrypt(raw);
    return parse(plain);
  }

  /** Returns the raw, still-encrypted 192-byte response from the device. Useful for debugging. */
  getRawEncrypted(): Promise<Buffer> {
    return this.sendCommand('getva', 192);
  }

  /** Navigates the device's on-screen display to the previous page. */
  async previousPage(): Promise<void> {
    await this.sendCommand('lastp', 0);
  }

  /** Navigates the device's on-screen display to the next page. */
  async nextPage(): Promise<void> {
    await this.sendCommand('nextp', 0);
  }

  /** Rotates the device's on-screen display. */
  async rotateScreen(): Promise<void> {
    await this.sendCommand('rotat', 0);
  }

  /** Lists the serial ports currently visible to the operating system. */
  static async listPorts(): Promise<string[]> {
    const ports = await SerialPort.list();
    return ports.map((p) => p.path);
  }

  private sendCommand(command: string, expectedLength: number): Promise<Buffer> {
    return new Promise((resolve, reject) => {
      if (!this.port.isOpen) {
        reject(new Error('Not connected. Call connect() first.'));
        return;
      }

      const chunks: Buffer[] = [];
      let received = 0;

      const cleanup = () => {
        clearTimeout(timer);
        this.port.off('data', onData);
      };

      const timer = setTimeout(() => {
        cleanup();
        reject(new Tc66ProtocolError(`Timed out waiting for response to '${command}'.`));
      }, this.timeoutMs);

      const onData = (chunk: Buffer) => {
        chunks.push(chunk);
        received += chunk.length;
        if (received >= expectedLength) {
          cleanup();
          resolve(Buffer.concat(chunks).subarray(0, expectedLength));
        }
      };

      if (expectedLength > 0) {
        this.port.on('data', onData);
      }

      this.port.write(command, (err) => {
        if (err) {
          cleanup();
          reject(err);
        } else if (expectedLength <= 0) {
          cleanup();
          resolve(Buffer.alloc(0));
        }
      });
    });
  }
}
