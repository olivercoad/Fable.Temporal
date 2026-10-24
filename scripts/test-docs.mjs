import { spawnSync } from 'node:child_process';
import { mkdirSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const out = resolve(root, 'artifacts/doc-examples');
mkdirSync(out, { recursive: true });
const files = ['README.md', ...readdirSync(resolve(root, 'docs')).filter(f => f.endsWith('.md')).sort().map(f => `docs/${f}`)];
const modules = [];
for (const file of files) {
  const markdown = readFileSync(resolve(root, file), 'utf8');
  for (const match of markdown.matchAll(/^```fsharp\r?\n([\s\S]*?)^```/gm)) {
    const body = match[1].trim();
    const setup = /let temporal\b/.test(body) ? '' :
      'let temporal: TemporalImplementation = import "Temporal" "temporal-polyfill"\n';
    const source = `open Fable.Core.JsInterop\nopen Fable.Temporal\n${setup}${body}`;
    modules.push(`module Example${modules.length} =\n${source.split('\n').map(line => `    ${line}`).join('\n')}`);
  }
}
if (modules.length === 0) throw new Error('No F# documentation examples found');
writeFileSync(resolve(out, 'Examples.fs'), `module DocumentationExamples\n\n${modules.join('\n\n')}\n`);
writeFileSync(resolve(out, 'Examples.fsproj'), `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Fable.Temporal/Fable.Temporal.fsproj" />
    <Compile Include="Examples.fs" />
  </ItemGroup>
</Project>
`);
for (const [command, args] of [
  ['dotnet', ['fable', resolve(out, 'Examples.fsproj'), '--outDir', resolve(out, 'dist')]],
  [process.execPath, [resolve(out, 'dist/Examples.js')]],
]) {
  const result = spawnSync(command, args, { cwd: root, stdio: 'inherit', timeout: 120000 });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status ?? 1);
}
console.log(`Passed ${modules.length} documentation examples`);
