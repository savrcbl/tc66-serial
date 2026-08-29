# Changelog

All notable changes to both packages are documented here. Versions are kept in sync
between the NuGet (`Tc66Serial`) and npm (`tc66-serial`) packages where practical.

## [0.1.0] - Unreleased

### Added

- Initial release.
- `Tc66Client` for both .NET and Node.js: connect, query mode, get a full reading,
  page/rotate the on-device display, list available ports.
- AES-256-ECB decryption and CRC-16/MODBUS validation of the device's `getva` response.
- Full field parsing: voltage, current, power, resistance, D+/D- voltage, temperature,
  and both mAh/mWh accumulator groups.
