"""The main client class for talking to a TC66 / TC66C device over serial."""

from __future__ import annotations

from typing import List, Optional

import serial
from serial.tools import list_ports

from .codec import decrypt, parse
from .exceptions import Tc66ProtocolError
from .types import Tc66Reading


class Tc66Client:
    """A client for reading measurements from a Ruideng (RD/RDTech) TC66 or TC66C
    USB power meter over a serial connection.

    Not thread-safe -- use one :class:`Tc66Client` instance per device and avoid
    calling its methods concurrently from multiple threads.

    Can be used as a context manager, which connects on entry and disconnects on
    exit::

        with Tc66Client("/dev/ttyACM0") as client:
            reading = client.get_reading()
    """

    def __init__(self, port: str, baud_rate: int = 115200, timeout: float = 3.0) -> None:
        """Creates a client for the given serial port. Call :meth:`connect` before use.

        Args:
            port: The OS device name, e.g. "COM10" on Windows or "/dev/ttyACM0" on
                Linux/macOS.
            baud_rate: Baud rate to use. TC66 devices default to 115200.
            timeout: Read/write timeout, in seconds.
        """
        self._port_name = port
        self._serial = serial.Serial()
        self._serial.port = port
        self._serial.baudrate = baud_rate
        self._serial.bytesize = serial.EIGHTBITS
        self._serial.parity = serial.PARITY_NONE
        self._serial.stopbits = serial.STOPBITS_ONE
        self._serial.timeout = timeout
        self._serial.write_timeout = timeout

    @property
    def port(self) -> str:
        """The serial port name this client was constructed with."""
        return self._port_name

    @property
    def is_connected(self) -> bool:
        """True if the underlying serial port is currently open."""
        return self._serial.is_open

    def connect(self) -> None:
        """Opens the underlying serial connection."""
        if not self._serial.is_open:
            self._serial.open()

    def disconnect(self) -> None:
        """Closes the underlying serial connection, if open."""
        if self._serial.is_open:
            self._serial.close()

    def query_mode(self) -> str:
        """Queries the device's current display mode."""
        self._ensure_connected()
        buf = self._send_command("query", 4)
        return buf.decode("ascii", errors="replace").strip("\x00 ")

    def get_reading(self) -> Tc66Reading:
        """Requests, decrypts and parses a full measurement snapshot from the device.

        Raises:
            Tc66ProtocolError: If the response is malformed.
        """
        self._ensure_connected()
        raw = self._send_command("getva", 192)
        plain = decrypt(raw)
        return parse(plain)

    def get_raw_encrypted(self) -> bytes:
        """Returns the raw, still-encrypted 192-byte response from the device.
        Useful for debugging.
        """
        self._ensure_connected()
        return self._send_command("getva", 192)

    def previous_page(self) -> None:
        """Navigates the device's on-screen display to the previous page."""
        self._ensure_connected()
        self._send_command("lastp", 0)

    def next_page(self) -> None:
        """Navigates the device's on-screen display to the next page."""
        self._ensure_connected()
        self._send_command("nextp", 0)

    def rotate_screen(self) -> None:
        """Rotates the device's on-screen display."""
        self._ensure_connected()
        self._send_command("rotat", 0)

    @staticmethod
    def get_available_ports() -> List[str]:
        """Lists the serial ports currently visible to the operating system."""
        return [p.device for p in list_ports.comports()]

    def _send_command(self, command: str, expected_length: int) -> bytes:
        self._serial.reset_input_buffer()
        self._serial.reset_output_buffer()
        self._serial.write(command.encode("ascii"))

        if expected_length <= 0:
            return b""

        buffer = bytearray()
        while len(buffer) < expected_length:
            chunk = self._serial.read(expected_length - len(buffer))
            if not chunk:
                break
            buffer.extend(chunk)

        if len(buffer) < expected_length:
            raise Tc66ProtocolError(
                f"Only received {len(buffer)}/{expected_length} bytes in response to '{command}'."
            )

        return bytes(buffer)

    def _ensure_connected(self) -> None:
        if not self.is_connected:
            raise RuntimeError("Not connected. Call connect() first.")

    def close(self) -> None:
        """Alias for :meth:`disconnect`, for parity with other Python file-like APIs."""
        self.disconnect()

    def __enter__(self) -> "Tc66Client":
        self.connect()
        return self

    def __exit__(self, exc_type: Optional[type], exc_val: Optional[BaseException], exc_tb: object) -> None:
        self.disconnect()
