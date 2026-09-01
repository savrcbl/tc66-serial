"""Exception types raised by tc66_serial."""


class Tc66ProtocolError(Exception):
    """Raised when a TC66 device returns a response that cannot be decrypted or
    parsed, or that fails its CRC-16/MODBUS checksum.
    """
