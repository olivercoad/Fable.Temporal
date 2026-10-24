# Fable.Temporal

F# bindings for JavaScript Temporal, with typed units, immutable date/time values, nanosecond precision, and explicit calendar and time-zone operations.

This library targets Fable JavaScript. It does not implement Temporal on .NET or convert through `System.DateTime`, `TimeSpan`, or JavaScript `Date`. Applications supply a native Temporal implementation or a polyfill.

## Get started

Install the NuGet package in your Fable application and add a Temporal implementation:

```sh
dotnet add YourApp.fsproj package Fable.Temporal
pnpm add temporal-polyfill
```

Import the implementation and use its factories:

```fsharp
open Fable.Core.JsInterop
open Fable.Temporal

let temporal: TemporalImplementation = import "Temporal" "temporal-polyfill"

let date = temporal.PlainDate.Create(2024, 2, 29)
let tomorrow = date.Add(DurationFields(0u, days = 1u))
let text = tomorrow.ToJSON()
if not (text = "2024-03-01") then failwith "Unexpected result"
```

For a runtime that already provides `globalThis.Temporal`, use `TemporalRuntime.Native`. It resolves the global on each access and throws if absent. `TemporalRuntime.IsNativeAvailable` checks for the global; a globally installed polyfill also satisfies that check.

The `temporal-polyfill` import uses native Temporal when available and otherwise supplies ISO and Gregorian calendar support. Use `temporal-polyfill/full` for additional calendars. These imports do not change globals. See the [package entry points](https://github.com/fullcalendar/temporal-polyfill/blob/main/polyfill/README.md#package-entrypoints).

## Pick a type

- `Instant`: an exact point on the timeline.
- `ZonedDateTime`: an instant with a time zone and calendar.
- `PlainDate`: a calendar date.
- `PlainTime`: a wall-clock time.
- `PlainDateTime`: a date and wall-clock time without a time zone.
- `PlainYearMonth` and `PlainMonthDay`: partial or recurring calendar values.
- `Duration`: calendar and time components, whose elapsed length can depend on context.

Methods such as `Add`, `With`, and `Round` return new values.

## Read further

- [Working with Temporal](https://github.com/olivercoad/Fable.Temporal/blob/main/docs/guide.md): construction, replacement, arithmetic, rounding, DST, and formatting.
- [F# semantics](https://github.com/olivercoad/Fable.Temporal/blob/main/docs/fsharp.md): errors, equality, sorting, collection keys, numeric precision, and emitted JavaScript.
- [Development](https://github.com/olivercoad/Fable.Temporal/blob/main/docs/development.md): tools, tests, documentation examples, packaging, and releases.
- [Changelog](https://github.com/olivercoad/Fable.Temporal/blob/main/CHANGELOG.md): release history.

Examples in the guides assume `open Fable.Temporal` and the `temporal` import shown above. Each example is otherwise independent.

The bindings follow the [Temporal API](https://tc39.es/proposal-temporal/docs/) and use [Fable's JavaScript interop](https://fable.io/docs/javascript/features.html). The selected implementation and host supply calendar, locale, and time-zone data.

[MIT license](https://github.com/olivercoad/Fable.Temporal/blob/main/LICENSE).
