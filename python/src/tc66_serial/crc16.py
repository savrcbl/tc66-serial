"""CRC-16/MODBUS implementation used to validate the 64-byte packets returned by
Ruideng (RD/RDTech) TC66 devices."""


def crc16_modbus(data: bytes, offset: int = 0, length: int | None = None) -> int:
    """Computes the CRC-16/MODBUS checksum over ``length`` bytes of ``data``,
    starting at ``offset``. If ``length`` is omitted, reads to the end of ``data``.
    """
    if length is None:
        length = len(data) - offset

    crc = 0xFFFF
    for i in range(offset, offset + length):
        crc ^= data[i]
        for _ in range(8):
            if crc & 0x0001:
                crc = (crc >> 1) ^ 0xA001
            else:
                crc >>= 1
    return crc & 0xFFFF


def validate_packet(packet: bytes) -> bool:
    """Validates a 64-byte TC66 packet. The CRC-16/MODBUS checksum is computed over
    the first 60 bytes and compared against the little-endian uint32 stored at
    offset 60 (only the lower 16 bits of which are meaningful).
    """
    if len(packet) < 64:
        return False
    stored = int.from_bytes(packet[60:64], "little")
    computed = crc16_modbus(packet, 0, 60)
    return stored == computed
