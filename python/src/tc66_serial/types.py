"""Data model for a decoded TC66 measurement snapshot."""

from dataclasses import dataclass


@dataclass(frozen=True)
class Tc66Reading:
    """A single decoded measurement snapshot read from a TC66 / TC66C device."""

    # pac1
    product_name: str
    """Product name reported by the device (e.g. "TC66")."""

    version: str
    """Firmware version string reported by the device (e.g. "1.18")."""

    serial_number: int
    """Device serial number."""

    run_count: int
    """Number of measurement runs recorded by the device."""

    voltage: float
    """Bus voltage, in volts."""

    current: float
    """Bus current, in amperes."""

    power: float
    """Bus power, in watts."""

    pac1_checksum_valid: bool
    """True if the first 64-byte packet passed its CRC-16/MODBUS check."""

    # pac2
    resistance: float
    """Estimated resistance of the load, in ohms."""

    group0_mah: float
    """Accumulated capacity for measurement group 0, in milliamp-hours."""

    group0_mwh: float
    """Accumulated energy for measurement group 0, in milliwatt-hours."""

    group1_mah: float
    """Accumulated capacity for measurement group 1, in milliamp-hours."""

    group1_mwh: float
    """Accumulated energy for measurement group 1, in milliwatt-hours."""

    temperature_negative: bool
    """True if the reported temperature is negative."""

    temperature: int
    """Unsigned temperature magnitude reported by the device. Units (C or F)
    depend on the device's display setting."""

    d_plus_voltage: float
    """USB D+ line voltage, in volts."""

    d_minus_voltage: float
    """USB D- line voltage, in volts."""

    pac2_checksum_valid: bool
    """True if the second 64-byte packet passed its CRC-16/MODBUS check."""

    # pac3
    pac3_checksum_valid: bool
    """True if the third 64-byte packet passed its CRC-16/MODBUS check."""

    @property
    def signed_temperature(self) -> int:
        """Signed temperature, combining ``temperature_negative`` and ``temperature``."""
        return -self.temperature if self.temperature_negative else self.temperature

    @property
    def is_valid(self) -> bool:
        """True if all three packets passed their CRC-16/MODBUS checks."""
        return self.pac1_checksum_valid and self.pac2_checksum_valid and self.pac3_checksum_valid
