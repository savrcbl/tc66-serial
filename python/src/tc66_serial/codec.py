"""Decrypts and parses the 192-byte response returned by a TC66 device's "getva"
command.
"""

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes

from .crc16 import validate_packet
from .exceptions import Tc66ProtocolError
from .types import Tc66Reading

# Fixed AES-256 key used by TC66 / TC66C devices to encrypt their measurement packets.
_AES_KEY = bytes(
    [
        0x58, 0x21, 0xFA, 0x56, 0x01, 0xB2, 0xF0, 0x26,
        0x87, 0xFF, 0x12, 0x04, 0x62, 0x2A, 0x4F, 0xB0,
        0x86, 0xF4, 0x02, 0x60, 0x81, 0x6F, 0x9A, 0x0B,
        0xA7, 0xF1, 0x06, 0x61, 0x9A, 0xB8, 0x72, 0x88,
    ]
)


def decrypt(cipher_text: bytes) -> bytes:
    """Decrypts a raw 192-byte response using AES-256-ECB (no padding)."""
    cipher = Cipher(algorithms.AES(_AES_KEY), modes.ECB())
    decryptor = cipher.decryptor()
    return decryptor.update(cipher_text) + decryptor.finalize()


def _read_ascii(data: bytes, offset: int, length: int) -> str:
    raw = data[offset : offset + length]
    return raw.split(b"\x00")[0].decode("ascii", errors="replace").strip()


def _read_u32(data: bytes, offset: int) -> int:
    return int.from_bytes(data[offset : offset + 4], "little")


def parse(plain_text: bytes) -> Tc66Reading:
    """Parses a decrypted 192-byte buffer (three 64-byte packets: "pac1", "pac2",
    "pac3") into a :class:`Tc66Reading`.

    Raises:
        Tc66ProtocolError: If the buffer is the wrong size or the packet headers
            are invalid.
    """
    if len(plain_text) < 192:
        raise Tc66ProtocolError(f"Expected 192 decrypted bytes, got {len(plain_text)}.")

    pac1 = plain_text[0:64]
    pac2 = plain_text[64:128]
    pac3 = plain_text[128:192]

    h1 = _read_ascii(pac1, 0, 4)
    h2 = _read_ascii(pac2, 0, 4)
    h3 = _read_ascii(pac3, 0, 4)

    if h1 != "pac1" or h2 != "pac2" or h3 != "pac3":
        raise Tc66ProtocolError(
            f"Unexpected packet headers [{h1}] [{h2}] [{h3}]. "
            "Decryption may have failed, or the response was truncated/corrupted."
        )

    return Tc66Reading(
        # pac1
        product_name=_read_ascii(pac1, 4, 4),
        version=_read_ascii(pac1, 8, 4),
        serial_number=_read_u32(pac1, 12),
        run_count=_read_u32(pac1, 44),
        voltage=_read_u32(pac1, 48) * 1e-4,
        current=_read_u32(pac1, 52) * 1e-5,
        power=_read_u32(pac1, 56) * 1e-4,
        pac1_checksum_valid=validate_packet(pac1),
        # pac2
        resistance=_read_u32(pac2, 4) * 1e-2,
        group0_mah=_read_u32(pac2, 8),
        group0_mwh=_read_u32(pac2, 12),
        group1_mah=_read_u32(pac2, 16),
        group1_mwh=_read_u32(pac2, 20),
        temperature_negative=_read_u32(pac2, 24) == 1,
        temperature=_read_u32(pac2, 28),
        d_plus_voltage=_read_u32(pac2, 32) * 1e-2,
        d_minus_voltage=_read_u32(pac2, 36) * 1e-2,
        pac2_checksum_valid=validate_packet(pac2),
        # pac3
        pac3_checksum_valid=validate_packet(pac3),
    )
