"""tc66_serial: read live measurements from Ruideng (RD/RDTech) TC66/TC66C USB
power meters over a serial connection.
"""

from .client import Tc66Client
from .codec import decrypt, parse
from .crc16 import crc16_modbus, validate_packet
from .exceptions import Tc66ProtocolError
from .types import Tc66Reading

__version__ = "0.1.0"

__all__ = [
    "Tc66Client",
    "Tc66Reading",
    "Tc66ProtocolError",
    "decrypt",
    "parse",
    "crc16_modbus",
    "validate_packet",
]
