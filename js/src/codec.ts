import { createDecipheriv } from 'node:crypto';
import { validatePacket } from './crc16.js';
import { Tc66ProtocolError, type Tc66Reading } from './types.js';

// Fixed AES-256 key used by TC66 / TC66C devices to encrypt their measurement packets.
const AES_KEY = Buffer.from([
  0x58, 0x21, 0xfa, 0x56, 0x01, 0xb2, 0xf0, 0x26, 0x87, 0xff, 0x12, 0x04, 0x62, 0x2a, 0x4f, 0xb0,
  0x86, 0xf4, 0x02, 0x60, 0x81, 0x6f, 0x9a, 0x0b, 0xa7, 0xf1, 0x06, 0x61, 0x9a, 0xb8, 0x72, 0x88,
]);

/** Decrypts a raw 192-byte response using AES-256-ECB (no padding). */
export function decrypt(cipherText: Buffer): Buffer {
  const decipher = createDecipheriv('aes-256-ecb', AES_KEY, null);
  decipher.setAutoPadding(false);
  return Buffer.concat([decipher.update(cipherText), decipher.final()]);
}

function readAscii(data: Buffer, offset: number, length: number): string {
  return data.toString('ascii', offset, offset + length).replace(/\0/g, '').trim();
}

/**
 * Parses a decrypted 192-byte buffer (three 64-byte packets: "pac1", "pac2", "pac3")
 * into a {@link Tc66Reading}.
 */
export function parse(plainText: Buffer): Tc66Reading {
  if (plainText.length < 192) {
    throw new Tc66ProtocolError(`Expected 192 decrypted bytes, got ${plainText.length}.`);
  }

  const pac1 = plainText.subarray(0, 64);
  const pac2 = plainText.subarray(64, 128);
  const pac3 = plainText.subarray(128, 192);

  const h1 = readAscii(pac1, 0, 4);
  const h2 = readAscii(pac2, 0, 4);
  const h3 = readAscii(pac3, 0, 4);

  if (h1 !== 'pac1' || h2 !== 'pac2' || h3 !== 'pac3') {
    throw new Tc66ProtocolError(
      `Unexpected packet headers [${h1}] [${h2}] [${h3}]. ` +
        'Decryption may have failed, or the response was truncated/corrupted.',
    );
  }

  return {
    // pac1
    productName: readAscii(pac1, 4, 4),
    version: readAscii(pac1, 8, 4),
    serialNumber: pac1.readUInt32LE(12),
    runCount: pac1.readUInt32LE(44),
    voltage: pac1.readUInt32LE(48) * 1e-4,
    current: pac1.readUInt32LE(52) * 1e-5,
    power: pac1.readUInt32LE(56) * 1e-4,
    pac1ChecksumValid: validatePacket(pac1 as Buffer),

    // pac2
    resistance: pac2.readUInt32LE(4) * 1e-2,
    group0Mah: pac2.readUInt32LE(8),
    group0Mwh: pac2.readUInt32LE(12),
    group1Mah: pac2.readUInt32LE(16),
    group1Mwh: pac2.readUInt32LE(20),
    temperatureNegative: pac2.readUInt32LE(24) === 1,
    temperature: pac2.readUInt32LE(28),
    dPlusVoltage: pac2.readUInt32LE(32) * 1e-2,
    dMinusVoltage: pac2.readUInt32LE(36) * 1e-2,
    pac2ChecksumValid: validatePacket(pac2 as Buffer),

    // pac3
    pac3ChecksumValid: validatePacket(pac3 as Buffer),
  };
}
