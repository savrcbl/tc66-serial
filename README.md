# tc66-serial

Libraries for talking to **RDTech/FNIRSI TC66** and **TC66C** USB power meters over their
serial (virtual COM port) interface — decrypting and parsing the device's live
measurement stream into voltage, current, power, resistance, D+/D- line voltage, and
capacity/energy counters.

This repo contains two packages, built from the same reverse-engineered protocol:

| Package | Ecosystem | Path |
|---|---|---|
| [`Tc66Serial`](dotnet/Tc66Serial) | NuGet (.NET) | `dotnet/Tc66Serial` |
| [`tc66-serial`](js) | npm (Node.js) | `js` |

> **Disclaimer:** This is an independent, community reverse-engineered client. It is not
> affiliated with, endorsed by, or supported by RDTech, FNIRSI, or Ruideng. The wire
> protocol was derived by observing device traffic and may not be complete or may change
> across firmware versions.

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

Both clients expose the same operations: `Connect`/`connect`, `Disconnect`/`disconnect`,
`QueryMode`/`queryMode`, `GetReading`/`getReading`, `PreviousPage`/`previousPage`,
`NextPage`/`nextPage`, `RotateScreen`/`rotateScreen`, and a static `GetAvailablePorts` /
`listPorts` helper.

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
[`dotnet/Tc66Serial/Tc66Codec.cs`](dotnet/Tc66Serial/Tc66Codec.cs) or
[`js/src/codec.ts`](js/src/codec.ts) for exact byte offsets and scaling factors.

## Repository layout

```
dotnet/Tc66Serial/   .NET library source (targets net8.0 and netstandard2.0)
js/                   TypeScript library source (built with tsup, dual ESM/CJS)
examples/             Minimal usage examples for both packages
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

## Publishing

Both packages are published from GitHub Actions when a tag matching `v*` is pushed
(e.g. `v0.1.0`) — see [`.github/workflows`](.github/workflows). You'll need to add two
repository secrets before this will work:

- `NUGET_API_KEY` — an API key from [nuget.org](https://www.nuget.org/account/apikeys)
- `NPM_TOKEN` — an automation token from [npmjs.com](https://www.npmjs.com/settings/~/tokens)

Bump the versions in `dotnet/Tc66Serial/Tc66Serial.csproj` and `js/package.json`, commit,
tag, and push:

```sh
git tag v0.1.0
git push origin v0.1.0
```

## Contributing

Issues and pull requests are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
