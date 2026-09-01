# tc66-serial

Libraries for talking to **Ruideng (RD/RDTech) TC66** and **TC66C** USB power meters over their
serial (virtual COM port) interface — decrypting and parsing the device's live
measurement stream into voltage, current, power, resistance, D+/D- line voltage, and
capacity/energy counters.

This repo contains two packages, built from the same reverse-engineered protocol:

| Package | Ecosystem | Path |
|---|---|---|
| [`Tc66Serial`](dotnet/Tc66Serial) | NuGet (.NET) | `dotnet/Tc66Serial` |
| [`tc66-serial`](js) | npm (Node.js) | `js` |
| [`tc66-serial`](python) | PyPI (Python) | `python` |

> **Disclaimer:** This is an independent, community reverse-engineered client. It is not
> affiliated with, endorsed by, or supported by Hangzhou Ruideng Technology Co., Ltd.
> (also commonly seen online as "RD" or "RDTech"). The wire protocol was derived by
> observing device traffic and may not be complete or may change across firmware versions.

## Supported devices

- TC66
- TC66C

Other RDTech USB testers that use the same "getva" / AES-256-ECB packet format may also
work, but are untested.

## Installation

**.NET:**

```sh
dotnet add package Tc66Serial
```

**Node.js:**

```sh
npm install tc66-serial
```

**Python:**

```sh
pip install tc66-serial
```

## Quick start

### .NET

```csharp
using Tc66Serial;

using var client = new Tc66Client("COM10"); // or "/dev/ttyACM0" on Linux
client.Connect();

Tc66Reading reading = client.GetReading();
Console.WriteLine($"{reading.Voltage:F4} V  {reading.Current:F5} A  {reading.Power:F4} W");
Console.WriteLine($"Checksums valid: {reading.IsValid}");
```

### Node.js / TypeScript

```ts
import { Tc66Client } from 'tc66-serial';

const client = new Tc66Client('/dev/ttyACM0'); // or 'COM10' on Windows
await client.connect();

const reading = await client.getReading();
console.log(`${reading.voltage.toFixed(4)} V  ${reading.current.toFixed(5)} A  ${reading.power.toFixed(4)} W`);

await client.disconnect();
```

### Python

```python
from tc66_serial import Tc66Client

with Tc66Client("/dev/ttyACM0") as client:  # or "COM10" on Windows
    reading = client.get_reading()
    print(f"{reading.voltage:.4f} V  {reading.current:.5f} A  {reading.power:.4f} W")
```

Both clients expose the same operations: `Connect`/`connect`, `Disconnect`/`disconnect`,
`QueryMode`/`queryMode`, `GetReading`/`getReading`, `PreviousPage`/`previousPage`,
`NextPage`/`nextPage`, `RotateScreen`/`rotateScreen`, and a static `GetAvailablePorts` /
`listPorts` helper. The Python client follows the same shape with `snake_case` names
(`get_reading`, `previous_page`, etc.) and also works as a context manager.

Runnable examples for both languages are in [`examples/`](examples).

## The protocol, briefly

- The device accepts short ASCII commands over serial at 115200 8N1 (`getva`, `query`,
  `lastp`, `nextp`, `rotat`).
- `getva` returns 192 bytes, encrypted with **AES-256-ECB** (no padding) under a fixed
  key embedded in the vendor's own Windows/Android apps.
- Decrypting those 192 bytes yields three 64-byte packets, each starting with an ASCII
  tag (`pac1`, `pac2`, `pac3`) and ending with a **CRC-16/MODBUS** checksum (computed
  over the first 60 bytes, stored zero-extended as a little-endian `uint32` at offset 60).
- `pac1` carries product name, firmware version, serial number, run count, voltage,
  current, and power. `pac2` carries resistance, the two mAh/mWh accumulator groups,
  temperature, and D+/D- line voltage. `pac3` is currently unused padding.

See the field-by-field offset tables in
[`dotnet/Tc66Serial/Tc66Codec.cs`](dotnet/Tc66Serial/Tc66Codec.cs),
[`js/src/codec.ts`](js/src/codec.ts), or
[`python/src/tc66_serial/codec.py`](python/src/tc66_serial/codec.py) for exact byte
offsets and scaling factors.

## Repository layout

```
dotnet/Tc66Serial/   .NET library source (targets net8.0 and netstandard2.0)
js/                   TypeScript library source (built with tsup, dual ESM/CJS)
python/               Python library source (packaged with hatchling)
examples/             Minimal usage examples for all three packages
.github/workflows/    CI: build/pack on every push, publish on tagged releases
```

## Building from source

**.NET:**

```sh
cd dotnet/Tc66Serial
dotnet build
dotnet pack -c Release
```

**Node.js:**

```sh
cd js
npm install
npm run build
```

**Python:**

```sh
cd python
python -m pip install build
python -m build
```

## Publishing

All three packages are published from GitHub Actions when a tag matching `v*` is pushed
(e.g. `v0.1.0`) — see [`.github/workflows`](.github/workflows).

- **NuGet** uses [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
  (OIDC, no long-lived key). Create a policy at nuget.org → your username → *Trusted
  Publishing* pointing at this repo and the `dotnet.yml` workflow file, then add a
  `NUGET_USER` repository secret containing your nuget.org username.
- **npm** still uses a token: add an `NPM_TOKEN` repository secret — an automation token
  from [npmjs.com](https://www.npmjs.com/settings/~/tokens).
- **PyPI** also uses Trusted Publishing (OIDC, no token). On pypi.org, go to your account
  → Publishing, and add a pending publisher: PyPI project name `tc66-serial`, owner
  `YOUR_GITHUB_USERNAME`, repository `tc66-serial`, workflow `python.yml`, environment
  `pypi`. No GitHub secret is needed for this one.

Bump the version in `dotnet/Tc66Serial/Tc66Serial.csproj`, `js/package.json`, and
`python/pyproject.toml`, commit, tag, and push:

```sh
git tag v0.1.0
git push origin v0.1.0
```

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
