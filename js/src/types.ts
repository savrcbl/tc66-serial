/** A single decoded measurement snapshot read from a TC66 / TC66C device. */
export interface Tc66Reading {
  /** Product name reported by the device (e.g. "TC66"). */
  productName: string;
  /** Firmware version string reported by the device (e.g. "1.18"). */
  version: string;
  /** Device serial number. */
  serialNumber: number;
  /** Number of measurement runs recorded by the device. */
  runCount: number;

  /** Bus voltage, in volts. */
  voltage: number;
  /** Bus current, in amperes. */
  current: number;
  /** Bus power, in watts. */
  power: number;

  /** Estimated resistance of the load, in ohms. */
  resistance: number;

  /** Accumulated capacity for measurement group 0, in milliamp-hours. */
  group0Mah: number;
  /** Accumulated energy for measurement group 0, in milliwatt-hours. */
  group0Mwh: number;
  /** Accumulated capacity for measurement group 1, in milliamp-hours. */
  group1Mah: number;
  /** Accumulated energy for measurement group 1, in milliwatt-hours. */
  group1Mwh: number;

  /** True if the reported temperature is negative. */
  temperatureNegative: boolean;
  /** Unsigned temperature magnitude reported by the device. Units (C or F) depend on the device's display setting. */
  temperature: number;

  /** USB D+ line voltage, in volts. */
  dPlusVoltage: number;
  /** USB D- line voltage, in volts. */
  dMinusVoltage: number;

  /** True if the first 64-byte packet passed its CRC-16/MODBUS check. */
  pac1ChecksumValid: boolean;
  /** True if the second 64-byte packet passed its CRC-16/MODBUS check. */
  pac2ChecksumValid: boolean;
  /** True if the third 64-byte packet passed its CRC-16/MODBUS check. */
  pac3ChecksumValid: boolean;
}

/** Returns the signed temperature, combining `temperatureNegative` and `temperature`. */
export function getSignedTemperature(reading: Tc66Reading): number {
  return reading.temperatureNegative ? -reading.temperature : reading.temperature;
}

/** True if all three packets in a reading passed their CRC-16/MODBUS checks. */
export function isValid(reading: Tc66Reading): boolean {
  return reading.pac1ChecksumValid && reading.pac2ChecksumValid && reading.pac3ChecksumValid;
}

/** Options accepted by the {@link Tc66Client} constructor. */
export interface Tc66ClientOptions {
  /** Baud rate to use. TC66 devices default to 115200. */
  baudRate?: number;
  /** Time to wait for a response before rejecting, in milliseconds. Defaults to 3000. */
  timeoutMs?: number;
}

/** Thrown when a TC66 device returns a response that cannot be decrypted, parsed, or read in time. */
export class Tc66ProtocolError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'Tc66ProtocolError';
  }
}
