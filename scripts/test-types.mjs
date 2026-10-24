import { spawnSync } from 'node:child_process';
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const out = resolve(root, 'artifacts/typecheck');
mkdirSync(out, { recursive: true });
const assembly = resolve(root, 'tests/bin/Debug/net10.0');
const reference = name => `#r @"${resolve(assembly, name).replaceAll('"', '""')}"`;
const header = `${reference('Fable.Core.dll')}\n${reference('Fable.Temporal.dll')}
open Fable.Core
open Fable.Temporal
let value = function Ok value -> value | Error error -> failwith error
`;
const cases = JSON.parse(readFileSync(resolve(root, 'tests/typecheck/cases.json'), 'utf8'));
let count = 0;
for (const fixture of cases) {
  const path = resolve(out, `${fixture.name}.fsx`);
  const body = fixture.code.split('\n').map(line => `    ${line}`).join('\n');
  writeFileSync(path, `${header}\nlet example (t: TemporalRuntime) =\n${body}\n`);
  const result = spawnSync('dotnet', ['fsi', '--nologo', '--readline-', '--exec', path], {
    cwd: root, encoding: 'utf8', timeout: 30000,
  });
  const output = `${result.stdout ?? ''}${result.stderr ?? ''}`;
  if (result.error) throw result.error;
  const passed = fixture.valid ? result.status === 0 : result.status !== 0 && /error FS\d+/.test(output);
  writeFileSync(resolve(out, `${fixture.name}.log`), output);
  if (!passed) throw new Error(`${fixture.name}: expected ${fixture.valid ? 'acceptance' : 'compiler rejection'}\n${output}`);
  count++;
}
console.log(`Passed ${count} compilation fixtures (${cases.filter(c => !c.valid).length} rejected, ${cases.filter(c => c.valid).length} accepted)`);
