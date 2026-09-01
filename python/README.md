# tc66-serial

A library for reading live measurements from Ruideng (RD/RDTech) **TC66** and
**TC66C** USB power meters over a serial connection: voltage, current, power,
resistance, D+/D- line voltage, and the two mAh/mWh accumulator groups.

```python
from tc66_serial import Tc66Client

with Tc66Client("/dev/ttyACM0") as client:  # or "COM10" on Windows
    reading = client.get_reading()
    print(f"{reading.voltage:.4f} V  {reading.current:.5f} A  {reading.power:.4f} W")
```

Full documentation, protocol notes, and the companion .NET and npm packages live
in the [GitHub repository](https://github.com/savrcbl/tc66-serial).
