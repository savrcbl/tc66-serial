#!/usr/bin/env python3
"""Minimal example: connect, take one reading, print it.

Run with:
    python index.py /dev/ttyACM0
(or COM10 on Windows)

From a fresh clone, install the library first:
    cd python && pip install -e .
"""

import sys

from tc66_serial import Tc66Client


def main() -> int:
    if len(sys.argv) < 2:
        print("Usage: python index.py <port> [baudRate]")
        return 1

    port = sys.argv[1]
    baud_rate = int(sys.argv[2]) if len(sys.argv) > 2 else 115200

    with Tc66Client(port, baud_rate=baud_rate) as client:
        print(f"Connected to {port} at {baud_rate} baud.")

        reading = client.get_reading()

        print(f"Device        : {reading.product_name} (firmware {reading.version}, serial {reading.serial_number})")
        print(f"Voltage       : {reading.voltage:.4f} V")
        print(f"Current       : {reading.current:.5f} A")
        print(f"Power         : {reading.power:.4f} W")
        print(f"Resistance    : {reading.resistance:.2f} Ohm")
        print(f"Group 0       : {reading.group0_mah} mAh / {reading.group0_mwh} mWh")
        print(f"Group 1       : {reading.group1_mah} mAh / {reading.group1_mwh} mWh")
        print(f"Temperature   : {reading.signed_temperature}")
        print(f"D+/D-         : {reading.d_plus_voltage:.2f} V / {reading.d_minus_voltage:.2f} V")
        print(f"Checksums OK  : {reading.is_valid}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
