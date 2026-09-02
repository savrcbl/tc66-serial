"""tc66_serial: read live measurements from Ruideng (RD/RDTech) TC66/TC66C USB
power meters over a serial connection.
"""

from importlib.metadata import PackageNotFoundError, version as _version

from .client import Tc66Client
from .codec import decrypt, parse
from .crc16 import crc16_modbus, validate_packet
from .exceptions import Tc66ProtocolError
from .types import Tc66Reading

try:
    __version__ = _version("tc66-serial")
except PackageNotFoundError:
    # Package isn't installed (e.g. running straight from a source checkout
    # without `pip install -e .` first) -- fall back rather than raise.
    __version__ = "0.0.0-dev"

__all__ = [
    "Tc66Client",
    "Tc66Reading",
    "Tc66ProtocolError",
    "decrypt",
    "parse",
    "crc16_modbus",
    "validate_packet",
]
