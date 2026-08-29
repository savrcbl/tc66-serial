# tc66-serial

A library for reading live measurements from RDTech/FNIRSI **TC66** and **TC66C** USB
power meters over a serial connection: voltage, current, power, resistance, D+/D- line
voltage, and the two mAh/mWh accumulator groups.

```ts
import { Tc66Client } from 'tc66-serial';

const client = new Tc66Client('/dev/ttyACM0'); // or 'COM10' on Windows
await client.connect();

const reading = await client.getReading();
console.log(`${reading.voltage.toFixed(4)} V  ${reading.current.toFixed(5)} A  ${reading.power.toFixed(4)} W`);

await client.disconnect();
```

Full documentation, protocol notes, and the companion .NET package live in the
[GitHub repository](https://github.com/YOUR_GITHUB_USERNAME/tc66-serial).
