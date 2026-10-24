# Development

Use the .NET SDK selected by `global.json`, Node 26, and pnpm 10.32.1 (pinned in `package.json`). CI runs on Node 26 and checks native Temporal support.

## Prepare the checkout

```sh
dotnet tool restore
pnpm install --frozen-lockfile
```

The repository pins Fable and Fantomas in `dotnet-tools.json`. The JavaScript polyfill dependency is listed in `package.json` and locked in `pnpm-lock.yaml`.

## Validate changes

```sh
pnpm run build
pnpm test
pnpm run test:types
pnpm run test:compiler-flag
pnpm run test:docs
pnpm run format:check
pnpm run pack
pnpm run test:package
```

- `build` checks the library and tests with the F# compiler.
- `test` compiles with Fable and executes against `temporal-polyfill`. It also exercises native Temporal when available.
- `test:types` compiles independent positive and negative fixtures from `tests/typecheck/cases.json`. Generated fixtures and diagnostics go to `artifacts/typecheck`.
- `test:compiler-flag` runs the behavioral suite with Fable's experimental Temporal mapping enabled.
- `test:docs` compiles and executes the F# examples in the README and guides using the `temporal-polyfill` entry point. Each block gets its own module and the documented `temporal` import.
- `format:check` checks the library and test source with Fantomas. Use `pnpm run format` to apply formatting.
- `pack` creates the NuGet package in `artifacts` without publishing it.
- `test:package` packs the library, restores it into a fresh package cache in a separate consumer project, and runs the behavioral suite through that package. Source mapping forces `Fable.Temporal` to come from the local artifact. This checks the package's Fable sources and executable helpers as well as its public API.

`pnpm run test:native` requires an existing global Temporal implementation and fails if none is available. Installing a polyfill globally is not a substitute for this native check.

The behavioral tests cover emitted representations as well as API results: erased cases, object-shaped options, exact nanoseconds, calendar and DST behavior, error classification, and value comparison. Type fixtures check restrictions that runtime tests cannot establish.

## Repository layout

`src/Fable.Temporal/Temporal.fs` contains the public bindings and helpers. Its XML comments provide editor documentation. `tests/Tests.fs` is the executable behavioral suite. `scripts` contains the native, compilation-fixture, and documentation runners.

Keep examples independent except for the shared import described in the README. Documentation examples throw when an expected result does not match; avoid examples that require native globals or depend on today's date.

## Package contents

The package contains the reference assembly, XML documentation, Fable source and project files, README, changelog, and guides. Applications still supply their own Temporal implementation.

`Fable.Package.SDK` manages Fable source inclusion, documentation generation, and Fable package tags. The package uses `FablePackageType=library` because it includes executable F# helpers alongside the bindings; those helpers need to be compiled by Fable. Its supported target is `fable-javascript`. The SDK is a private build dependency and is not a dependency of the published package.

`Ionide.KeepAChangelog.Tasks` reads the root `CHANGELOG.md` and sets the package version and NuGet release notes from its latest versioned entry. It is a private build dependency. Keep changes under `Unreleased` until preparing a release.

When adding source files, update F# compilation order in the project file. When changing the API, update examples and compilation fixtures together. Check emitted JavaScript when introducing erasure or emit expressions: a successful .NET build alone cannot validate their runtime representation.

## Publish a release

1. Move the changes under `Unreleased` in `CHANGELOG.md` into a new `## [<version>] - YYYY-MM-DD` entry and update the comparison links. The version and NuGet release notes come from this entry.
2. Run the validation commands above and push the changes. Confirm CI passes on Node 26, including the native Temporal check.
3. Create and publish a GitHub release with a tag matching `v<version>` against the intended commit. Use the corresponding changelog entry for the release notes. Publishing this GitHub release triggers NuGet publication.
4. The release workflow checks the tag, reruns CI, attaches the tested `.nupkg` to the GitHub release, and publishes that same artifact to NuGet through trusted publishing. Its `publish` job uses the `nuget` environment.
5. Confirm the package is listed on NuGet and install it in a Fable application. NuGet indexing can take time after a successful push.

NuGet package versions cannot be overwritten. If a published package needs a correction, bump the version and create a new release. The workflow tolerates an already-published version when rerunning a release.
