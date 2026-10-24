import { spawnSync } from 'node:child_process';
import { copyFileSync, mkdirSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const out = resolve(root, 'artifacts/package-consumer');
const metadata = spawnSync('dotnet', [
  'msbuild', 'src/Fable.Temporal/Fable.Temporal.fsproj', '-nologo',
  '-target:SetVersionFromChangelog', '-getProperty:PackageVersion',
], { cwd: root, encoding: 'utf8', timeout: 30000 });
if (metadata.error) throw metadata.error;
if (metadata.status !== 0) throw new Error(metadata.stderr || metadata.stdout);
const version = metadata.stdout.trim();
if (!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(version)) {
  throw new Error(`Invalid changelog package version: ${version}`);
}

// A fresh package cache prevents an older build of the same version from passing.
rmSync(out, { recursive: true, force: true });
mkdirSync(out, { recursive: true });
copyFileSync(resolve(root, 'tests/Tests.fs'), resolve(out, 'Tests.fs'));
writeFileSync(resolve(out, 'Consumer.fsproj'), `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Fable.Temporal" Version="${version}" />
    <Compile Include="Tests.fs" />
  </ItemGroup>
</Project>
`);
writeFileSync(resolve(out, 'NuGet.Config'), `<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value=".." />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local"><package pattern="Fable.Temporal" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
`);

const env = { ...process.env, NUGET_PACKAGES: resolve(out, 'packages') };
for (const [command, args] of [
  ['dotnet', ['restore', 'Consumer.fsproj', '--configfile', 'NuGet.Config']],
  ['dotnet', ['fable', 'Consumer.fsproj', '--outDir', 'dist']],
  [process.execPath, ['dist/Tests.js']],
]) {
  const result = spawnSync(command, args, { cwd: out, env, stdio: 'inherit', timeout: 180000 });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status ?? 1);
}
console.log(`Passed packaged NuGet consumer for Fable.Temporal ${version}`);
