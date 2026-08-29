// Minimal example: connect, take one reading, print it.
//
// Run with:
//   node examples/node-cli/index.mjs /dev/ttyACM0
// (or COM10 on Windows)
//
// From a fresh clone, build the library first:
//   cd js && npm install && npm run build

import { Tc66Client } from '../../js/dist/index.js';

const [port, baudRateArg] = process.argv.slice(2);

if (!port) {
  console.error('Usage: node index.mjs <port> [baudRate]');
  process.exit(1);
}

const baudRate = baudRateArg ? Number(baudRateArg) : 115200;

const client = new Tc66Client(port, { baudRate });
await client.connect();
console.log(`Connected to ${port} at ${baudRate} baud.`);

const reading = await client.getReading();

console.log(`Device        : ${reading.productName} (firmware ${reading.version}, serial ${reading.serialNumber})`);
console.log(`Voltage       : ${reading.voltage.toFixed(4)} V`);
console.log(`Current       : ${reading.current.toFixed(5)} A`);
console.log(`Power         : ${reading.power.toFixed(4)} W`);
console.log(`Resistance    : ${reading.resistance.toFixed(2)} Ohm`);
console.log(`Group 0       : ${reading.group0Mah} mAh / ${reading.group0Mwh} mWh`);
console.log(`Group 1       : ${reading.group1Mah} mAh / ${reading.group1Mwh} mWh`);
console.log(`D+/D-         : ${reading.dPlusVoltage.toFixed(2)} V / ${reading.dMinusVoltage.toFixed(2)} V`);
console.log(`Checksums OK  : ${reading.pac1ChecksumValid && reading.pac2ChecksumValid && reading.pac3ChecksumValid}`);

await client.disconnect();
