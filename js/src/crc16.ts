/**
 * Computes the CRC-16/MODBUS checksum over `length` bytes of `data`, starting at `offset`.
 */
export function crc16Modbus(data: Buffer, offset: number, length: number): number {
  let crc = 0xffff;
  for (let i = offset; i < offset + length; i++) {
    crc ^= data[i];
    for (let bit = 0; bit < 8; bit++) {
      if ((crc & 0x0001) !== 0) {
        crc = (crc >> 1) ^ 0xa001;
      } else {
        crc >>= 1;
      }
    }
  }
  return crc & 0xffff;
}

/**
 * Validates a 64-byte TC66 packet. The CRC-16/MODBUS checksum is computed over the first
 * 60 bytes and compared against the little-endian uint32 stored at offset 60 (only the
 * lower 16 bits of which are meaningful).
 */
export function validatePacket(packet: Buffer): boolean {
  if (packet.length < 64) return false;
  const stored = packet.readUInt32LE(60);
  const computed = crc16Modbus(packet, 0, 60);
  return stored === computed;
}
