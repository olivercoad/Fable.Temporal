# F# semantics

These bindings expose the selected implementation's JavaScript objects. F# types make operation shapes explicit, while Temporal handles date validity and runtime arithmetic.

## Handle errors

Direct calls throw JavaScript errors. F# `try/with` can catch them, but .NET exception classes do not describe JavaScript's `RangeError` and `TypeError`.

Factories provide `TryParse`, field-based `TryFrom`, and `TryCreate` methods returning `Result<_, TemporalError>`:

```fsharp
let result =
    temporal.PlainDate.TryFrom(
        DateFields(2024, 2, 30),
        OverflowOptions(overflow = Overflow.Reject))

match result with
| Ok date -> failwithf "Expected rejection, got %s" (date.ToJSON())
| Error (TemporalError.RangeError _) -> ()
| Error error -> raise error.Cause
```

Every factory has `TryParse` and `TryCreate`; factories accepting field bags also have `TryFrom` for those bags. Instant epoch factories have `TryFromEpochNanoseconds` and `TryFromEpochMilliseconds`. Duration factories also provide `TryCreateTime` and `TryCreateFixed`, preserving their narrower duration types.

Plain dates and date-times provide `TryToZonedDateTime`; instants provide `TryToZonedDateTimeISO`. All arguments and defaults match the corresponding throwing operation:

```fsharp
let local = temporal.PlainDateTime.Parse("2024-04-07T02:30")
let resolved =
    local.TryToZonedDateTime(
        "Australia/Melbourne",
        DisambiguationOptions(disambiguation = Disambiguation.Reject))
match resolved with
| Error (TemporalError.RangeError _) -> ()
| Error error -> raise error.Cause
| Ok _ -> failwith "Expected ambiguous time to be rejected"
```

These are F# extension methods available with `open Fable.Temporal`. They call the existing Temporal operation through `Temporal.tryCall`; no JavaScript `tryParse` or `tryCreate` method is required.

For other operations, `Temporal.tryCall` provides the same result boundary:

```fsharp
let time = temporal.PlainTime.Parse("12:37")
let rounded =
    Temporal.tryCall (fun () ->
        time.Round(TimeRoundOptions(TimeUnit.Minute, roundingIncrement = 7)))
match rounded with
| Error (TemporalError.RangeError _) -> ()
| Error error -> raise error.Cause
| Ok _ -> failwith "Expected invalid increment to be rejected"
```

`TemporalError.RangeError`, `.TypeError`, and `.Other` each retain the original exception. The `Cause` property retrieves it without a match. Classification uses the JavaScript error's `name`; it does not rely on realm-specific constructors or prove where the error originated.

Keep the callback focused. Application failures inside it are caught too, and unknown thrown values remain `Other`. The helper does not await promises or catch later promise rejections.

Parsing returns the same classified errors:

```fsharp
let parsed = temporal.PlainDate.TryParse("2024-02-29")
match parsed with
| Ok date -> if not (date.Day = 29) then failwith "Unexpected result"
| Error error -> raise error.Cause
```

An error boundary does not change defaults. If an operation successfully constrains an invalid day, the result is `Ok`. Request `Overflow.Reject` to reject that adjustment.

## Compare values

Use the curried module functions with collection operations:

```fsharp
let first = temporal.Instant.Create(0I)
let second = temporal.Instant.Create(1I)
let sorted = [ second; first ] |> List.sortWith Instant.compare
if not ((List.head sorted).EpochNanoseconds = 0I) then failwith "Unexpected result"
if not (sorted |> List.exists (Instant.equals (temporal.Instant.Create(0I)))) then failwith "Unexpected result"
```

`Instant`, `PlainDate`, `PlainTime`, `PlainDateTime`, `PlainYearMonth`, and `ZonedDateTime` have `equals` and `compare`. Comparison returns -1, 0, or 1. Instance `IsEqualTo` and factory `Compare` offer the same Temporal operations.

`PlainMonthDay` has equality but no comparison: Temporal does not define an ordering for recurring month/day values.

General duration comparison needs context:

```fsharp
let anchor = RelativeTo.Date (temporal.PlainDate.Create(2024, 2, 29))
let month = temporal.Duration.Parse("P1M")
let thirtyDays = temporal.Duration.Parse("P30D")
if not (Duration.compare anchor month thirtyDays < 0) then failwith "Unexpected result"

let hour = temporal.Duration.CreateTime(hours = 1u)
let minutes = temporal.Duration.CreateTime(minutes = 60u)
if not (FixedDuration.compare hour minutes = 0) then failwith "Unexpected result"
```

Duration comparison measures elapsed length; it does not compare component representations. The fixed-duration helper needs no anchor.

## Choose collection identity explicitly

Temporal comparison and equality can answer different questions. Zoned ordering compares instants, while equality also checks time zone and calendar:

```fsharp
let zoned = temporal.ZonedDateTime.Parse("2024-01-01T12:00+11:00[Australia/Melbourne]")
let utc = zoned.WithTimeZone("UTC")
if not (ZonedDateTime.compare zoned utc = 0) then failwith "Unexpected result"
if not (not (ZonedDateTime.equals zoned utc)) then failwith "Unexpected result"
```

Plain date ordering similarly ignores calendar identity, while equality includes it. These behaviors follow the [zoned](https://tc39.es/proposal-temporal/docs/zoneddatetime.html#compare) and [date](https://tc39.es/proposal-temporal/docs/plaindate.html#compare) contracts.

F# `=`, generic `compare`, `List.sort`, `List.distinct`, and default collection comparers do not become Temporal value operations. Use explicit helpers for predicates and sorting.

For `Map` and `Set`, choose a key that represents the identity your application needs. Epoch nanoseconds are exact instant keys:

```fsharp
let instant = temporal.Instant.Parse("2024-01-01T00:00:00Z")
let copy = temporal.Instant.From(instant)
let unique =
    [ instant; copy ]
    |> List.map (fun value -> value.EpochNanoseconds)
    |> Set.ofList
if not (Set.count unique = 1) then failwith "Unexpected result"
```

The same key intentionally ignores a zoned value's calendar and zone. A display string is not a general-purpose identity key; even time-zone identifiers can have equivalent spellings.

## Preserve numeric precision

`EpochNanoseconds` and `FromEpochNanoseconds` use F# `bigint`, compiled to JavaScript `bigint`. They preserve the full supported Temporal epoch range:

```fsharp
let instant = temporal.Instant.Parse("2024-01-01T00:00:00.123456789Z")
let restored = temporal.Instant.FromEpochNanoseconds(instant.EpochNanoseconds)
if not (Instant.equals instant restored) then failwith "Unexpected result"
```

Epoch milliseconds, duration getters, and totals use `float` because Temporal exposes JavaScript numbers. JavaScript numbers represent integers exactly only through magnitude 9,007,199,254,740,991. Converting epoch nanoseconds to `float` can discard precision.

Typed duration construction uses `uint32` magnitudes. Parsing and capability refinement can retain larger durations without narrowing their components to `uint32`. Persist exact values as Temporal strings or decimal epoch-nanosecond strings; raw JavaScript bigint is not directly JSON-serializable.

## Understand the binding boundary

F# rejects mismatched unit families, incomplete construction bags, incompatible patches, and general durations passed to instant arithmetic. Temporal still checks parsed strings, numeric ranges, calendars, time zones, rounding increments, unit ordering, and arithmetic overflow. Explicit casts or fabricated interop objects can bypass the typed API.

Missing properties use `option` where appropriate. In particular, the binding converts a missing time-zone transition from JavaScript `null` to `None`.

The library must run as Fable-generated JavaScript. A successful .NET build checks types; it does not supply a .NET Temporal implementation.

The emitted code has two layers:

- Interfaces, string-enum policies, numeric precision cases, named relative anchors, and option bags use direct JavaScript representations. Inline equality and comparison helpers call the value or its constructor; comparison uses the left value's implementation.
- `TemporalRuntime` allocates a wrapper and checks its implementation. Patch builders allocate replacement objects from F# change unions. Error handling, including the `Try*` extension methods, and duration refinement execute checks and produce results. `WithMagnitude` constructs signed replacement fields.

Using `TemporalImplementation` directly avoids the runtime wrapper. The helper layer can introduce Fable runtime imports and function allocations; the library does not promise that every feature is erased or that bundles contain zero supporting code.
