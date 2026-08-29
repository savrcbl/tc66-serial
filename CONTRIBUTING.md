# Contributing

Thanks for considering a contribution!

## Reporting protocol quirks

If you have a TC66/TC66C on a firmware version this library doesn't decode correctly,
please open an issue with:

- The device model and firmware version (from `QueryMode`/`queryMode`, or the label on
  the device's info screen)
- A hex dump of both the raw and decrypted 192-byte response, if possible (the .NET
  example CLI and the Node example CLI can both print this — see `examples/`)
- What you expected vs. what you got

## Development

**.NET** (`dotnet/Tc66Serial`):

```sh
dotnet build
dotnet test   # if/when tests are added
```

**Node.js** (`js`):

```sh
npm install
npm run typecheck
npm run build
```

## Pull requests

- Keep the .NET and TypeScript implementations in sync — they intentionally mirror each
  other's field names, offsets, and behavior so both packages describe the same protocol.
- Please describe how you tested a change against real hardware, since this library talks
  to a physical device and CI cannot exercise the actual serial link.
