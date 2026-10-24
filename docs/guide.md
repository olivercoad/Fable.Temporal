# Working with Temporal

Examples use the implementation imported in the [README](../README.md).

## Construct and parse

Factories separate constructor arguments, strings, and field bags. Use their `TryParse`, `TryCreate`, and field-based `TryFrom` counterparts when you want a [classified Result](fsharp.md#handle-errors):

- `Create` invokes the Temporal constructor. Date constructor arguments use ISO calendar fields, even when a different calendar is selected.
- `Parse` reads a Temporal string.
- `From` accepts a complete field bag or an existing value.

```fsharp
let leap = temporal.PlainDate.Parse("2024-02-29")
let date =
    temporal.PlainDate.From(
        DateFields(2024, 2, 29),
        OverflowOptions(overflow = Overflow.Reject))
if not (PlainDate.equals leap date) then failwith "Unexpected result"
```

`DateFields` and `DateTimeFields` require year, month, and day. `ZonedDateTimeFields` also requires a time zone. `YearMonthFields` requires year and month; `MonthDayFields` requires a month code and day. `TimeFields` requires an hour. Optional clock fields default to zero.

Month codes such as `"M02"` identify calendar months; they are not always interchangeable with numeric month positions in calendars with leap months. Calendar identifiers, month codes, and time-zone identifiers remain strings because the implementation validates their meaning.

For field-based construction, the default overflow policy is `Constrain`: an out-of-range day can be adjusted to the last day of the month. Pass `Overflow.Reject` when adjustment is unacceptable. Invalid parsed strings still fail.

## Replace fields

Patches describe nonempty changes to an existing value. Each receiver has its own patch and change types:

```fsharp
let date = temporal.PlainDate.Create(2024, 2, 29)
let changed = date.With(DatePatch.Create(DateChange.Month 3, DateChange.Day 20))
if not (changed.ToJSON() = "2024-03-20") then failwith "Unexpected result"
if not (date.ToJSON() = "2024-02-29") then failwith "Unexpected result"
```

Use `DatePatch`, `TimePatch`, `DateTimePatch`, `YearMonthPatch`, `MonthDayPatch`, or `ZonedPatch` as appropriate. `Create` takes one required change and any additional changes. Repeating the same field keeps its last supplied value.

Patches cannot change a calendar or time zone. Use `WithCalendar` or `WithTimeZone` for those operations. Construction bags and patches are separate types.

## Add and subtract

A general duration can contain years, months, weeks, days, and clock units. Typed construction uses nonnegative `uint32` magnitudes; use `Negated()` or parse a signed string for negative durations.

```fsharp
let date = temporal.PlainDate.Create(2024, 2, 29)
let nextYear = date.Add(DurationFields(0u, years = 1u))
if not (nextYear.ToJSON() = "2025-02-28") then failwith "Unexpected result"

let instant = temporal.Instant.Parse("2024-01-01T00:00:00Z")
let elapsed = temporal.Duration.CreateTime(hours = 1u)
let later = instant.Add(elapsed)
if not (later.ToJSON() = "2024-01-01T01:00:00Z") then failwith "Unexpected result"
```

The duration types express which components are permitted:

- `Duration` can include calendar units. Totals, rounding, and comparison require a relative date or zoned date-time.
- `FixedDuration` excludes years, months, and weeks. Its context-free operations treat days as 24 hours.
- `TimeDuration` also excludes days. It can be added to an `Instant`.

`Duration.CreateFixed` and `Duration.CreateTime` construct the narrower types. `FixedDurationFields` requires days; `TimeDurationFields` requires hours; `DurationFields` requires seconds. Supply `0u` when that required component is zero.

An arbitrary parsed or computed duration can be refined after inspecting its components:

```fsharp
let parsed = temporal.Duration.Parse("-PT1H")
let instant = temporal.Instant.Create(0I)

match Duration.tryAsTime parsed with
| Ok elapsed -> if not ((instant.Add(elapsed)).EpochNanoseconds = -3600000000000I) then failwith "Unexpected result"
| Error reason -> failwith reason
```

`Duration.tryAsFixed` checks for absent years, months, and weeks; `tryAsTime` checks days too. They return `Result<_, string>` and retain the original value on success.

`WithMagnitude(DurationPatch.Create(...))` changes component magnitudes while preserving the receiver's sign. For a zero duration, supplied nonzero magnitudes produce a positive duration. Operations that could introduce additional components return a broader duration type; refine again before instant arithmetic.

## Time zones and daylight saving

Converting a local date-time to a zone can encounter a repeated or missing wall-clock time:

```fsharp
let local = temporal.PlainDateTime.Parse("2024-04-07T02:30")
let earlier =
    local.ToZonedDateTime(
        "Australia/Melbourne",
        DisambiguationOptions(disambiguation = Disambiguation.Earlier))
let later =
    local.ToZonedDateTime(
        "Australia/Melbourne",
        DisambiguationOptions(disambiguation = Disambiguation.Later))
if not (later.EpochNanoseconds - earlier.EpochNanoseconds = 3600000000000I) then failwith "Unexpected result"
```

`Disambiguation.Compatible` is the default: choose the earlier instant for repeated times and the later one for skipped times. `Reject` throws for either ambiguity.

`ZonedOptions` also accepts an `OffsetPolicy` for conflicts between a supplied offset and a zone. Zoned parsing and field construction default to `Reject`; zoned replacement defaults to `Prefer`.

A calendar day and 24 elapsed hours can differ across daylight-saving changes. Supply a zoned anchor when measuring a duration in that context:

```fsharp
let anchor = temporal.ZonedDateTime.Parse("2024-10-05T12:00+10:00[Australia/Melbourne]")
let day = temporal.Duration.Parse("P1D")
let hours = day.Total(TotalOptions(DateTimeUnit.Hour, RelativeTo.ZonedDateTime anchor))
let fixedHours = temporal.Duration.CreateFixed(days = 1u).Total(DayTimeUnit.Hour)
if not (hours = 23.) then failwith "Unexpected result"
if not (fixedHours = 24.) then failwith "Unexpected result"
```

`RelativeTo` has named `Text`, `Date`, `DateTime`, and `ZonedDateTime` cases. A plain date-time anchor supplies calendar context without time-zone transitions.

`GetTimeZoneTransition` returns `None` when no transition exists in the requested direction.

## Round and measure differences

Unit unions are specific to the operation. Options are direct constructors:

```fsharp
let time = temporal.PlainTime.Parse("12:37")
let rounded =
    time.Round(
        TimeRoundOptions(TimeUnit.Minute,
                         roundingIncrement = 15,
                         roundingMode = RoundingMode.HalfEven))
if not (rounded.ToJSON() = "12:30:00") then failwith "Unexpected result"

let start = temporal.PlainDate.Create(2024, 1, 1)
let difference = start.Until("2024-04-01", DateDifferenceOptions(largestUnit = DateUnit.Month))
if not (difference.Months = 3.) then failwith "Unexpected result"
```

Use `TimeRoundOptions` for instants and plain times, and `DayTimeRoundOptions` for plain/zoned date-times and fixed durations. Passing just a unit is also supported. General duration rounding uses `DurationRoundOptions`, with a required smallest unit and relative anchor.

Difference options are `TimeDifferenceOptions`, `DateDifferenceOptions`, `DateTimeDifferenceOptions`, and `YearMonthDifferenceOptions`. `Until` measures from the receiver to the argument; `Since` measures in the opposite direction. Omit `largestUnit` for Temporal's automatic choice.

The nine rounding modes are `Ceil`, `Floor`, `Expand`, `Trunc`, `HalfCeil`, `HalfFloor`, `HalfExpand`, `HalfTrunc`, and `HalfEven`. Rounding generally defaults to `HalfExpand`; differences and string formatting default to `Trunc`.

Options do not validate numbers when constructed. Temporal checks them when an operation runs. For example, minute rounding of a plain time accepts 15 but rejects 7, since the increment must divide 60. An instant can round to 120 seconds, while a plain time cannot. Reversed largest/smallest unit ordering also throws. Use the [error boundary](fsharp.md#handle-errors) when a `Result` is useful.

## Format and persist

Use `ToJSON()` for a Temporal persistence string. `ToString(options)` controls precision and annotations:

```fsharp
let instant = temporal.Instant.Parse("2024-01-01T00:00:00.123456789Z")
let text =
    instant.ToString(
        InstantFormatOptions(
            fractionalSecondDigits = FractionalSecondDigits.Digits FractionalDigits.Nine))
if not (text = "2024-01-01T00:00:00.123456789Z") then failwith "Unexpected result"
```

`FractionalSecondDigits.Auto` selects automatic precision; `Digits` takes `FractionalDigits.Zero` through `.Nine`. Duration formatting uses `DurationFormatOptions` and `DurationPrecisionUnit`, which start at seconds.

Locale formatting accepts one locale string or an array of preferred locales. Omit locales for the host default:

```fsharp
let date = temporal.PlainDate.Create(2024, 2, 29)
let display =
    date.ToLocaleString(
        DateStyleOptions(dateStyle = FormatStyle.Short),
        locales = [| "en-GB" |])
if not (display.Length > 0) then failwith "Unexpected result"
```

Component options and style options are separate types. For example, `DateLocaleOptions` selects individual date components, while `DateStyleOptions` selects a date style. Use the corresponding receiver-specific types for time, date-time, instant, year-month, month-day, and zoned values. Zoned options cannot override the value's time zone.

Intl fractional-second formatting accepts only `LocaleSecondDigits.One`, `.Two`, or `.Three`. Duration localization exposes global style and fractional digits, not per-component numeric styles. Locale output depends on the host's Intl data and should not be used as a persistence format.
