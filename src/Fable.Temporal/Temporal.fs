namespace Fable.Temporal

open Fable.Core
open Fable.Core.JsInterop

/// Select whether out-of-range calendar fields are constrained or rejected. Field operations default to Constrain.
[<StringEnum; RequireQualifiedAccess>]
type Overflow =
    | Constrain
    | Reject

/// Resolve repeated or skipped local times. Compatible chooses earlier for repeats and later for gaps.
[<StringEnum; RequireQualifiedAccess>]
type Disambiguation =
    | Compatible
    | Earlier
    | Later
    | Reject

/// Resolve disagreement between a supplied UTC offset and the selected time zone.
[<StringEnum; RequireQualifiedAccess>]
type OffsetPolicy =
    | Use
    | Prefer
    | Ignore
    | Reject

/// Choose rounding direction or tie-breaking behavior for Temporal arithmetic and formatting.
[<StringEnum; RequireQualifiedAccess>]
type RoundingMode =
    | Ceil
    | Floor
    | Expand
    | Trunc
    | HalfCeil
    | HalfFloor
    | HalfExpand
    | HalfTrunc
    | HalfEven

/// Clock units supported by instant and plain-time rounding and differences.
[<StringEnum; RequireQualifiedAccess>]
type TimeUnit =
    | Hour
    | Minute
    | Second
    | Millisecond
    | Microsecond
    | Nanosecond

/// Calendar units supported by plain-date differences.
[<StringEnum; RequireQualifiedAccess>]
type DateUnit =
    | Year
    | Month
    | Week
    | Day

/// Year and month units supported by year-month differences.
[<StringEnum; RequireQualifiedAccess>]
type YearMonthUnit =
    | Year
    | Month

/// Calendar and clock units for date-time differences and context-dependent durations.
[<StringEnum; RequireQualifiedAccess>]
type DateTimeUnit =
    | Year
    | Month
    | Week
    | Day
    | Hour
    | Minute
    | Second
    | Millisecond
    | Microsecond
    | Nanosecond

/// Day and clock units for date-time rounding and fixed-duration operations.
[<StringEnum; RequireQualifiedAccess>]
type DayTimeUnit =
    | Day
    | Hour
    | Minute
    | Second
    | Millisecond
    | Microsecond
    | Nanosecond

/// Smallest displayed unit for time, date-time, instant, and zoned string formatting.
[<StringEnum; RequireQualifiedAccess>]
type PrecisionUnit =
    | Minute
    | Second
    | Millisecond
    | Microsecond
    | Nanosecond

/// Control whether and how a calendar annotation appears in a Temporal string.
[<StringEnum; RequireQualifiedAccess>]
type CalendarName =
    | Auto
    | Always
    | Never
    | Critical

/// Control the time-zone annotation in a zoned Temporal string.
[<StringEnum; RequireQualifiedAccess>]
type TimeZoneName =
    | Auto
    | Never
    | Critical

/// Include or omit the UTC offset in a zoned Temporal string.
[<StringEnum; RequireQualifiedAccess>]
type OffsetDisplay =
    | Auto
    | Never

/// Search forward or backward for a time-zone offset transition.
[<StringEnum; RequireQualifiedAccess>]
type TransitionDirection =
    | Next
    | Previous

/// Select an Intl date or time style; availability depends on the receiver.
[<StringEnum; RequireQualifiedAccess>]
type FormatStyle =
    | Full
    | Long
    | Medium
    | Short

/// Display an Intl component numerically, optionally padded to two digits.
[<StringEnum; RequireQualifiedAccess>]
type NumericStyle =
    | Numeric
    | [<CompiledName("2-digit")>] TwoDigit

/// Display an Intl month numerically or using a localized name.
[<StringEnum; RequireQualifiedAccess>]
type MonthStyle =
    | Numeric
    | [<CompiledName("2-digit")>] TwoDigit
    | Long
    | Short
    | Narrow

/// Choose the length of a localized weekday or era name.
[<StringEnum; RequireQualifiedAccess>]
type TextStyle =
    | Long
    | Short
    | Narrow

/// Select the Intl 12-hour or 24-hour clock convention.
[<StringEnum; RequireQualifiedAccess>]
type HourCycle =
    | H11
    | H12
    | H23
    | H24

/// Choose how Intl resolves the requested locales.
[<StringEnum; RequireQualifiedAccess>]
type LocaleMatcher =
    | Lookup
    | [<CompiledName("best fit")>] BestFit

/// Choose how Intl selects a format for requested components.
[<StringEnum; RequireQualifiedAccess>]
type FormatMatcher =
    | Basic
    | [<CompiledName("best fit")>] BestFit

/// Select a localized time-zone name or offset display.
[<StringEnum; RequireQualifiedAccess>]
type ZoneFormatStyle =
    | Long
    | Short
    | ShortOffset
    | LongOffset
    | ShortGeneric
    | LongGeneric

/// Select a global Intl duration style.
[<StringEnum; RequireQualifiedAccess>]
type DurationFormatStyle =
    | Long
    | Short
    | Narrow
    | Digital

/// Supported duration string precision, from seconds through nanoseconds.
[<StringEnum; RequireQualifiedAccess>]
type DurationPrecisionUnit =
    | Second
    | Millisecond
    | Microsecond
    | Nanosecond

/// A precision count from zero through nine, emitted as a JavaScript number.
[<Erase; RequireQualifiedAccess>]
type FractionalDigits =
    | [<CompiledValue(0)>] Zero
    | [<CompiledValue(1)>] One
    | [<CompiledValue(2)>] Two
    | [<CompiledValue(3)>] Three
    | [<CompiledValue(4)>] Four
    | [<CompiledValue(5)>] Five
    | [<CompiledValue(6)>] Six
    | [<CompiledValue(7)>] Seven
    | [<CompiledValue(8)>] Eight
    | [<CompiledValue(9)>] Nine

/// Intl date/time precision counts, restricted to one through three.
[<Erase; RequireQualifiedAccess>]
type LocaleSecondDigits =
    | [<CompiledValue(1)>] One
    | [<CompiledValue(2)>] Two
    | [<CompiledValue(3)>] Three

/// Choose automatic string precision or a fixed fractional digit count.
[<Erase; RequireQualifiedAccess>]
type FractionalSecondDigits =
    | [<CompiledName("auto")>] Auto
    | Digits of FractionalDigits

/// A single date replacement field. Combine changes using DatePatch.Create.
[<RequireQualifiedAccess>]
type DateChange =
    | Year of int
    | Month of int
    | MonthCode of string
    | Day of int

/// A nonempty date patch. Repeated fields take their last supplied value.
[<Erase>]
type DatePatch =
    private
    | DatePatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: DateChange, [<System.ParamArray>] rest: DateChange array) =
        let property =
            function
            | DateChange.Year value -> "year", box value
            | DateChange.Month value -> "month", box value
            | DateChange.MonthCode value -> "monthCode", box value
            | DateChange.Day value -> "day", box value

        DatePatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single year-month replacement field. Combine changes using YearMonthPatch.Create.
[<RequireQualifiedAccess>]
type YearMonthChange =
    | Year of int
    | Month of int
    | MonthCode of string

/// A nonempty year-month patch. Repeated fields take their last supplied value.
[<Erase>]
type YearMonthPatch =
    private
    | YearMonthPatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: YearMonthChange, [<System.ParamArray>] rest: YearMonthChange array) =
        let property =
            function
            | YearMonthChange.Year value -> "year", box value
            | YearMonthChange.Month value -> "month", box value
            | YearMonthChange.MonthCode value -> "monthCode", box value

        YearMonthPatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single month-day replacement field. Combine changes using MonthDayPatch.Create.
[<RequireQualifiedAccess>]
type MonthDayChange =
    | Month of int
    | MonthCode of string
    | Day of int
    | Year of int

/// A nonempty month-day patch. Repeated fields take their last supplied value.
[<Erase>]
type MonthDayPatch =
    private
    | MonthDayPatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: MonthDayChange, [<System.ParamArray>] rest: MonthDayChange array) =
        let property =
            function
            | MonthDayChange.Month value -> "month", box value
            | MonthDayChange.MonthCode value -> "monthCode", box value
            | MonthDayChange.Day value -> "day", box value
            | MonthDayChange.Year value -> "year", box value

        MonthDayPatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single time replacement field. Combine changes using TimePatch.Create.
[<RequireQualifiedAccess>]
type TimeChange =
    | Hour of int
    | Minute of int
    | Second of int
    | Millisecond of int
    | Microsecond of int
    | Nanosecond of int

/// A nonempty time patch. Repeated fields take their last supplied value.
[<Erase>]
type TimePatch =
    private
    | TimePatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: TimeChange, [<System.ParamArray>] rest: TimeChange array) =
        let property =
            function
            | TimeChange.Hour value -> "hour", box value
            | TimeChange.Minute value -> "minute", box value
            | TimeChange.Second value -> "second", box value
            | TimeChange.Millisecond value -> "millisecond", box value
            | TimeChange.Microsecond value -> "microsecond", box value
            | TimeChange.Nanosecond value -> "nanosecond", box value

        TimePatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single local date-time replacement field. Combine changes using DateTimePatch.Create.
[<RequireQualifiedAccess>]
type DateTimeChange =
    | Year of int
    | Month of int
    | Day of int
    | Hour of int
    | Minute of int
    | Second of int
    | Millisecond of int
    | Microsecond of int
    | Nanosecond of int
    | MonthCode of string

/// A nonempty local date-time patch. Repeated fields take their last supplied value.
[<Erase>]
type DateTimePatch =
    private
    | DateTimePatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: DateTimeChange, [<System.ParamArray>] rest: DateTimeChange array) =
        let property =
            function
            | DateTimeChange.Year value -> "year", box value
            | DateTimeChange.Month value -> "month", box value
            | DateTimeChange.Day value -> "day", box value
            | DateTimeChange.Hour value -> "hour", box value
            | DateTimeChange.Minute value -> "minute", box value
            | DateTimeChange.Second value -> "second", box value
            | DateTimeChange.Millisecond value -> "millisecond", box value
            | DateTimeChange.Microsecond value -> "microsecond", box value
            | DateTimeChange.Nanosecond value -> "nanosecond", box value
            | DateTimeChange.MonthCode value -> "monthCode", box value

        DateTimePatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single zoned date-time replacement field. Combine changes using ZonedPatch.Create.
[<RequireQualifiedAccess>]
type ZonedChange =
    | Year of int
    | Month of int
    | Day of int
    | Hour of int
    | Minute of int
    | Second of int
    | Millisecond of int
    | Microsecond of int
    | Nanosecond of int
    | MonthCode of string
    | Offset of string

/// A nonempty zoned date-time patch. Repeated fields take their last supplied value.
[<Erase>]
type ZonedPatch =
    private
    | ZonedPatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: ZonedChange, [<System.ParamArray>] rest: ZonedChange array) =
        let property =
            function
            | ZonedChange.Year value -> "year", box value
            | ZonedChange.Month value -> "month", box value
            | ZonedChange.Day value -> "day", box value
            | ZonedChange.Hour value -> "hour", box value
            | ZonedChange.Minute value -> "minute", box value
            | ZonedChange.Second value -> "second", box value
            | ZonedChange.Millisecond value -> "millisecond", box value
            | ZonedChange.Microsecond value -> "microsecond", box value
            | ZonedChange.Nanosecond value -> "nanosecond", box value
            | ZonedChange.MonthCode value -> "monthCode", box value
            | ZonedChange.Offset value -> "offset", box value

        ZonedPatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// A single duration replacement field. Combine changes using DurationPatch.Create.
[<RequireQualifiedAccess>]
type DurationChange =
    | Years of uint32
    | Months of uint32
    | Weeks of uint32
    | Days of uint32
    | Hours of uint32
    | Minutes of uint32
    | Seconds of uint32
    | Milliseconds of uint32
    | Microseconds of uint32
    | Nanoseconds of uint32

/// A nonempty duration patch. Repeated fields take their last supplied value.
[<Erase>]
type DurationPatch =
    private
    | DurationPatch of obj

    /// Build a replacement object from at least one change; duplicate fields use the final change.
    static member Create(first: DurationChange, [<System.ParamArray>] rest: DurationChange array) =
        let property =
            function
            | DurationChange.Years value -> "years", box value
            | DurationChange.Months value -> "months", box value
            | DurationChange.Weeks value -> "weeks", box value
            | DurationChange.Days value -> "days", box value
            | DurationChange.Hours value -> "hours", box value
            | DurationChange.Minutes value -> "minutes", box value
            | DurationChange.Seconds value -> "seconds", box value
            | DurationChange.Milliseconds value -> "milliseconds", box value
            | DurationChange.Microseconds value -> "microseconds", box value
            | DurationChange.Nanoseconds value -> "nanoseconds", box value

        DurationPatch(
            createObj
                [
                    yield property first
                    for field in rest do
                        yield property field
                ]
        )

/// Construct a calendar date from required year, month, and day fields.
[<Global>]
type DateFields [<ParamObject; Emit("$0")>] (year: int, month: int, day: int, ?calendar: string) =
    member _.year: int = jsNative
    member _.month: int = jsNative
    member _.day: int = jsNative
    member _.calendar: string option = jsNative

/// Construct a clock time; the required hour may be zero and omitted smaller fields default to zero.
and [<Global>] TimeFields
    [<ParamObject; Emit("$0")>]
    (hour: int, ?minute: int, ?second: int, ?millisecond: int, ?microsecond: int, ?nanosecond: int) =
    member _.hour: int = jsNative
    member _.minute: int option = jsNative
    member _.second: int option = jsNative
    member _.millisecond: int option = jsNative
    member _.microsecond: int option = jsNative
    member _.nanosecond: int option = jsNative

/// Construct a local date-time from required date fields and optional clock fields.
and [<Global>] DateTimeFields
    [<ParamObject; Emit("$0")>]
    (
        year: int,
        month: int,
        day: int,
        ?hour: int,
        ?minute: int,
        ?second: int,
        ?millisecond: int,
        ?microsecond: int,
        ?nanosecond: int,
        ?calendar: string
    ) =
    member _.year: int = jsNative
    member _.month: int = jsNative
    member _.day: int = jsNative
    member _.hour: int option = jsNative
    member _.minute: int option = jsNative
    member _.second: int option = jsNative
    member _.millisecond: int option = jsNative
    member _.microsecond: int option = jsNative
    member _.nanosecond: int option = jsNative
    member _.calendar: string option = jsNative

/// Construct a calendar year-month from required year and month fields.
and [<Global>] YearMonthFields [<ParamObject; Emit("$0")>] (year: int, month: int, ?calendar: string) =
    member _.year: int = jsNative
    member _.month: int = jsNative
    member _.calendar: string option = jsNative

/// Construct a recurring date using a month code and day, without requiring a reference year.
and [<Global>] MonthDayFields [<ParamObject; Emit("$0")>] (monthCode: string, day: int, ?calendar: string) =
    member _.monthCode: string = jsNative
    member _.day: int = jsNative
    member _.calendar: string option = jsNative

/// Construct a zoned value from required date and zone fields, with optional time, offset, and calendar.
and [<Global>] ZonedDateTimeFields
    [<ParamObject; Emit("$0")>]
    (
        year: int,
        month: int,
        day: int,
        timeZone: string,
        ?hour: int,
        ?minute: int,
        ?second: int,
        ?millisecond: int,
        ?microsecond: int,
        ?nanosecond: int,
        ?offset: string,
        ?calendar: string
    ) =
    member _.year: int = jsNative
    member _.month: int = jsNative
    member _.day: int = jsNative
    member _.timeZone: string = jsNative
    member _.hour: int option = jsNative
    member _.minute: int option = jsNative
    member _.second: int option = jsNative
    member _.millisecond: int option = jsNative
    member _.microsecond: int option = jsNative
    member _.nanosecond: int option = jsNative
    member _.offset: string option = jsNative
    member _.calendar: string option = jsNative

/// Construct a general duration from unsigned component magnitudes; required seconds may be zero.
and [<Global>] DurationFields
    [<ParamObject; Emit("$0")>]
    (
        seconds: uint32,
        ?years: uint32,
        ?months: uint32,
        ?weeks: uint32,
        ?days: uint32,
        ?hours: uint32,
        ?minutes: uint32,
        ?milliseconds: uint32,
        ?microseconds: uint32,
        ?nanoseconds: uint32
    ) =
    member _.seconds: uint32 = jsNative
    member _.years: uint32 option = jsNative
    member _.months: uint32 option = jsNative
    member _.weeks: uint32 option = jsNative
    member _.days: uint32 option = jsNative
    member _.hours: uint32 option = jsNative
    member _.minutes: uint32 option = jsNative
    member _.milliseconds: uint32 option = jsNative
    member _.microseconds: uint32 option = jsNative
    member _.nanoseconds: uint32 option = jsNative

/// Supply unsigned clock components for instant-safe arithmetic; required hours may be zero.
and [<Global>] TimeDurationFields
    [<ParamObject; Emit("$0")>]
    (
        hours: uint32,
        ?minutes: uint32,
        ?seconds: uint32,
        ?milliseconds: uint32,
        ?microseconds: uint32,
        ?nanoseconds: uint32
    ) =
    member _.hours: uint32 = jsNative
    member _.minutes: uint32 option = jsNative
    member _.seconds: uint32 option = jsNative
    member _.milliseconds: uint32 option = jsNative
    member _.microseconds: uint32 option = jsNative
    member _.nanoseconds: uint32 option = jsNative

/// Supply unsigned day and clock components for fixed-duration arithmetic; required days may be zero.
and [<Global>] FixedDurationFields
    [<ParamObject; Emit("$0")>]
    (
        days: uint32,
        ?hours: uint32,
        ?minutes: uint32,
        ?seconds: uint32,
        ?milliseconds: uint32,
        ?microseconds: uint32,
        ?nanoseconds: uint32
    ) =
    member _.days: uint32 = jsNative
    member _.hours: uint32 option = jsNative
    member _.minutes: uint32 option = jsNative
    member _.seconds: uint32 option = jsNative
    member _.milliseconds: uint32 option = jsNative
    member _.microseconds: uint32 option = jsNative
    member _.nanoseconds: uint32 option = jsNative

/// Override calendar overflow handling; omission preserves the operation default.
and [<Global>] OverflowOptions [<ParamObject; Emit("$0")>] (?overflow: Overflow) =
    member _.overflow: Overflow option = jsNative

/// Override local-time ambiguity handling when resolving a time zone.
and [<Global>] DisambiguationOptions [<ParamObject; Emit("$0")>] (?disambiguation: Disambiguation) =
    member _.disambiguation: Disambiguation option = jsNative

/// Control field overflow, local-time ambiguity, and supplied-offset conflicts in zoned operations.
and [<Global>] ZonedOptions
    [<ParamObject; Emit("$0")>]
    (?overflow: Overflow, ?disambiguation: Disambiguation, ?offset: OffsetPolicy) =
    member _.overflow: Overflow option = jsNative
    member _.disambiguation: Disambiguation option = jsNative
    member _.offset: OffsetPolicy option = jsNative

/// Round an instant or plain time to a required clock unit. Temporal validates increments when used.
and [<Global>] TimeRoundOptions
    [<ParamObject; Emit("$0")>]
    (smallestUnit: TimeUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: TimeUnit = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Round to a required day or clock unit. Numeric restrictions depend on the receiver.
and [<Global>] DayTimeRoundOptions
    [<ParamObject; Emit("$0")>]
    (smallestUnit: DayTimeUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: DayTimeUnit = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Configure a difference in clock units. Omitted units retain Temporal defaults; numeric validation occurs at use.
and [<Global>] TimeDifferenceOptions
    [<ParamObject; Emit("$0")>]
    (?smallestUnit: TimeUnit, ?largestUnit: TimeUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: TimeUnit option = jsNative
    member _.largestUnit: TimeUnit option = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Configure a difference in calendar units. Omitted units retain Temporal defaults; numeric validation occurs at use.
and [<Global>] DateDifferenceOptions
    [<ParamObject; Emit("$0")>]
    (?smallestUnit: DateUnit, ?largestUnit: DateUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: DateUnit option = jsNative
    member _.largestUnit: DateUnit option = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Configure a difference in calendar and clock units. Omitted units retain Temporal defaults; numeric validation occurs at use.
and [<Global>] DateTimeDifferenceOptions
    [<ParamObject; Emit("$0")>]
    (?smallestUnit: DateTimeUnit, ?largestUnit: DateTimeUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: DateTimeUnit option = jsNative
    member _.largestUnit: DateTimeUnit option = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Configure a difference in year and month units. Omitted units retain Temporal defaults; numeric validation occurs at use.
and [<Global>] YearMonthDifferenceOptions
    [<ParamObject; Emit("$0")>]
    (?smallestUnit: YearMonthUnit, ?largestUnit: YearMonthUnit, ?roundingIncrement: int, ?roundingMode: RoundingMode) =
    member _.smallestUnit: YearMonthUnit option = jsNative
    member _.largestUnit: YearMonthUnit option = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Pass a parsed or textual calendar anchor directly to a duration operation.
and [<Erase; RequireQualifiedAccess>] RelativeTo =
    | Text of string
    | Date of PlainDate
    | DateTime of PlainDateTime
    | ZonedDateTime of ZonedDateTime

/// Provide the required calendar or zoned anchor for duration comparison.
and [<Global>] RelativeToOptions [<ParamObject; Emit("$0")>] (relativeTo: RelativeTo) =
    member _.relativeTo: RelativeTo = jsNative

/// Round a general duration using a required unit and anchor. Temporal checks unit ordering and increments.
and [<Global>] DurationRoundOptions
    [<ParamObject; Emit("$0")>]
    (
        smallestUnit: DateTimeUnit,
        relativeTo: RelativeTo,
        ?largestUnit: DateTimeUnit,
        ?roundingIncrement: int,
        ?roundingMode: RoundingMode
    ) =
    member _.smallestUnit: DateTimeUnit = jsNative
    member _.relativeTo: RelativeTo = jsNative
    member _.largestUnit: DateTimeUnit option = jsNative
    member _.roundingIncrement: int option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Measure a general duration in the selected unit relative to a required anchor.
and [<Global>] TotalOptions [<ParamObject; Emit("$0")>] (unit: DateTimeUnit, relativeTo: RelativeTo) =
    member _.unit: DateTimeUnit = jsNative
    member _.relativeTo: RelativeTo = jsNative

/// Control plain-time string precision and rounding.
and [<Global>] PrecisionOptions
    [<ParamObject; Emit("$0")>]
    (?fractionalSecondDigits: FractionalSecondDigits, ?smallestUnit: PrecisionUnit, ?roundingMode: RoundingMode) =
    member _.fractionalSecondDigits: FractionalSecondDigits option = jsNative
    member _.smallestUnit: PrecisionUnit option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Control duration string precision using seconds or smaller units.
and [<Global>] DurationFormatOptions
    [<ParamObject; Emit("$0")>]
    (?fractionalSecondDigits: FractionalSecondDigits, ?smallestUnit: DurationPrecisionUnit, ?roundingMode: RoundingMode)
    =
    member _.fractionalSecondDigits: FractionalSecondDigits option = jsNative
    member _.smallestUnit: DurationPrecisionUnit option = jsNative
    member _.roundingMode: RoundingMode option = jsNative

/// Control the calendar annotation when formatting a date or partial date.
and [<Global>] CalendarFormatOptions [<ParamObject; Emit("$0")>] (?calendarName: CalendarName) =
    member _.calendarName: CalendarName option = jsNative

/// Control local date-time string precision, rounding, and calendar annotation.
and [<Global>] DateTimeFormatOptions
    [<ParamObject; Emit("$0")>]
    (
        ?fractionalSecondDigits: FractionalSecondDigits,
        ?smallestUnit: PrecisionUnit,
        ?roundingMode: RoundingMode,
        ?calendarName: CalendarName
    ) =
    member _.fractionalSecondDigits: FractionalSecondDigits option = jsNative
    member _.smallestUnit: PrecisionUnit option = jsNative
    member _.roundingMode: RoundingMode option = jsNative
    member _.calendarName: CalendarName option = jsNative

/// Control instant string precision and optionally display it in a selected time zone.
and [<Global>] InstantFormatOptions
    [<ParamObject; Emit("$0")>]
    (
        ?fractionalSecondDigits: FractionalSecondDigits,
        ?smallestUnit: PrecisionUnit,
        ?roundingMode: RoundingMode,
        ?timeZone: string
    ) =
    member _.fractionalSecondDigits: FractionalSecondDigits option = jsNative
    member _.smallestUnit: PrecisionUnit option = jsNative
    member _.roundingMode: RoundingMode option = jsNative
    member _.timeZone: string option = jsNative

/// Control zoned string precision and calendar, zone, and offset annotations.
and [<Global>] ZonedFormatOptions
    [<ParamObject; Emit("$0")>]
    (
        ?fractionalSecondDigits: FractionalSecondDigits,
        ?smallestUnit: PrecisionUnit,
        ?roundingMode: RoundingMode,
        ?calendarName: CalendarName,
        ?timeZoneName: TimeZoneName,
        ?offset: OffsetDisplay
    ) =
    member _.fractionalSecondDigits: FractionalSecondDigits option = jsNative
    member _.smallestUnit: PrecisionUnit option = jsNative
    member _.roundingMode: RoundingMode option = jsNative
    member _.calendarName: CalendarName option = jsNative
    member _.timeZoneName: TimeZoneName option = jsNative
    member _.offset: OffsetDisplay option = jsNative

/// Select Intl components for dates; component options cannot be mixed with style options.
and [<Global>] DateLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?weekday: TextStyle,
        ?era: TextStyle,
        ?year: NumericStyle,
        ?month: MonthStyle,
        ?day: NumericStyle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.weekday: TextStyle option = jsNative
    member _.era: TextStyle option = jsNative
    member _.year: NumericStyle option = jsNative
    member _.month: MonthStyle option = jsNative
    member _.day: NumericStyle option = jsNative

/// Select Intl components for times; component options cannot be mixed with style options.
and [<Global>] TimeLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?hour12: bool,
        ?hourCycle: HourCycle,
        ?dayPeriod: TextStyle,
        ?hour: NumericStyle,
        ?minute: NumericStyle,
        ?second: NumericStyle,
        ?fractionalSecondDigits: LocaleSecondDigits
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative
    member _.dayPeriod: TextStyle option = jsNative
    member _.hour: NumericStyle option = jsNative
    member _.minute: NumericStyle option = jsNative
    member _.second: NumericStyle option = jsNative
    member _.fractionalSecondDigits: LocaleSecondDigits option = jsNative

/// Select Intl components for local date-times; component options cannot be mixed with style options.
and [<Global>] DateTimeLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?weekday: TextStyle,
        ?era: TextStyle,
        ?year: NumericStyle,
        ?month: MonthStyle,
        ?day: NumericStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle,
        ?dayPeriod: TextStyle,
        ?hour: NumericStyle,
        ?minute: NumericStyle,
        ?second: NumericStyle,
        ?fractionalSecondDigits: LocaleSecondDigits
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.weekday: TextStyle option = jsNative
    member _.era: TextStyle option = jsNative
    member _.year: NumericStyle option = jsNative
    member _.month: MonthStyle option = jsNative
    member _.day: NumericStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative
    member _.dayPeriod: TextStyle option = jsNative
    member _.hour: NumericStyle option = jsNative
    member _.minute: NumericStyle option = jsNative
    member _.second: NumericStyle option = jsNative
    member _.fractionalSecondDigits: LocaleSecondDigits option = jsNative

/// Select Intl components for year-months; component options cannot be mixed with style options.
and [<Global>] YearMonthLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?era: TextStyle,
        ?year: NumericStyle,
        ?month: MonthStyle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.era: TextStyle option = jsNative
    member _.year: NumericStyle option = jsNative
    member _.month: MonthStyle option = jsNative

/// Select Intl components for month-days; component options cannot be mixed with style options.
and [<Global>] MonthDayLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?month: MonthStyle,
        ?day: NumericStyle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.month: MonthStyle option = jsNative
    member _.day: NumericStyle option = jsNative

/// Select Intl components for instants; component options cannot be mixed with style options.
and [<Global>] InstantLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?weekday: TextStyle,
        ?era: TextStyle,
        ?year: NumericStyle,
        ?month: MonthStyle,
        ?day: NumericStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle,
        ?dayPeriod: TextStyle,
        ?hour: NumericStyle,
        ?minute: NumericStyle,
        ?second: NumericStyle,
        ?fractionalSecondDigits: LocaleSecondDigits,
        ?timeZone: string,
        ?timeZoneName: ZoneFormatStyle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.weekday: TextStyle option = jsNative
    member _.era: TextStyle option = jsNative
    member _.year: NumericStyle option = jsNative
    member _.month: MonthStyle option = jsNative
    member _.day: NumericStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative
    member _.dayPeriod: TextStyle option = jsNative
    member _.hour: NumericStyle option = jsNative
    member _.minute: NumericStyle option = jsNative
    member _.second: NumericStyle option = jsNative
    member _.fractionalSecondDigits: LocaleSecondDigits option = jsNative
    member _.timeZone: string option = jsNative
    member _.timeZoneName: ZoneFormatStyle option = jsNative

/// Select Intl components for zoned date-times; component options cannot be mixed with style options.
and [<Global>] ZonedLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?formatMatcher: FormatMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?weekday: TextStyle,
        ?era: TextStyle,
        ?year: NumericStyle,
        ?month: MonthStyle,
        ?day: NumericStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle,
        ?dayPeriod: TextStyle,
        ?hour: NumericStyle,
        ?minute: NumericStyle,
        ?second: NumericStyle,
        ?fractionalSecondDigits: LocaleSecondDigits,
        ?timeZoneName: ZoneFormatStyle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.formatMatcher: FormatMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.weekday: TextStyle option = jsNative
    member _.era: TextStyle option = jsNative
    member _.year: NumericStyle option = jsNative
    member _.month: MonthStyle option = jsNative
    member _.day: NumericStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative
    member _.dayPeriod: TextStyle option = jsNative
    member _.hour: NumericStyle option = jsNative
    member _.minute: NumericStyle option = jsNative
    member _.second: NumericStyle option = jsNative
    member _.fractionalSecondDigits: LocaleSecondDigits option = jsNative
    member _.timeZoneName: ZoneFormatStyle option = jsNative

/// Select Intl styles for dates without individual component settings.
and [<Global>] DateStyleOptions
    [<ParamObject; Emit("$0")>]
    (?localeMatcher: LocaleMatcher, ?numberingSystem: string, ?calendar: string, ?dateStyle: FormatStyle) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.dateStyle: FormatStyle option = jsNative

/// Select Intl styles for times without individual component settings.
and [<Global>] TimeStyleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?timeStyle: FormatStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.timeStyle: FormatStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative

/// Select Intl styles for local date-times without individual component settings.
and [<Global>] DateTimeStyleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?dateStyle: FormatStyle,
        ?timeStyle: FormatStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.dateStyle: FormatStyle option = jsNative
    member _.timeStyle: FormatStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative

/// Select Intl styles for instants without individual component settings.
and [<Global>] InstantStyleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?dateStyle: FormatStyle,
        ?timeStyle: FormatStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle,
        ?timeZone: string
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.dateStyle: FormatStyle option = jsNative
    member _.timeStyle: FormatStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative
    member _.timeZone: string option = jsNative

/// Select Intl styles for zoned date-times without individual component settings.
and [<Global>] ZonedStyleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?numberingSystem: string,
        ?calendar: string,
        ?dateStyle: FormatStyle,
        ?timeStyle: FormatStyle,
        ?hour12: bool,
        ?hourCycle: HourCycle
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.calendar: string option = jsNative
    member _.dateStyle: FormatStyle option = jsNative
    member _.timeStyle: FormatStyle option = jsNative
    member _.hour12: bool option = jsNative
    member _.hourCycle: HourCycle option = jsNative

/// Select a global duration display style and precision; component-specific numeric styles are not exposed.
and [<Global>] DurationLocaleOptions
    [<ParamObject; Emit("$0")>]
    (
        ?localeMatcher: LocaleMatcher,
        ?numberingSystem: string,
        ?style: DurationFormatStyle,
        ?fractionalDigits: FractionalDigits
    ) =
    member _.localeMatcher: LocaleMatcher option = jsNative
    member _.numberingSystem: string option = jsNative
    member _.style: DurationFormatStyle option = jsNative
    member _.fractionalDigits: FractionalDigits option = jsNative

/// Resolve a date in a required zone, optionally at a supplied wall-clock time.
and [<Global>] DateToZonedOptions [<ParamObject; Emit("$0")>] (timeZone: string, ?plainTime: PlainTime) =
    member _.timeZone: string = jsNative
    member _.plainTime: PlainTime option = jsNative

/// Supply the year needed to convert a recurring month/day to a complete date.
and [<Global>] YearFields [<ParamObject; Emit("$0")>] (year: int) =
    member _.year: int = jsNative

/// Supply the day needed to convert a year/month to a complete date.
and [<Global>] DayFields [<ParamObject; Emit("$0")>] (day: int) =
    member _.day: int = jsNative

/// An immutable timeline point with exact bigint epoch nanoseconds.
and Instant =
    [<Emit("$0.epochMilliseconds")>]
    abstract EpochMilliseconds: float

    [<Emit("$0.epochNanoseconds")>]
    abstract EpochNanoseconds: bigint

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: Instant -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.add($1)")>]
    abstract Add: duration: TimeDuration -> Instant

    [<Emit("$0.add($1)")>]
    abstract Add: duration: TimeDurationFields -> Instant

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: duration: TimeDuration -> Instant

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: duration: TimeDurationFields -> Instant

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: Instant * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: Instant * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.round($1)")>]
    abstract Round: options: TimeRoundOptions -> Instant

    [<Emit("$0.round($1)")>]
    abstract Round: smallestUnit: TimeUnit -> Instant

    [<Emit("$0.toZonedDateTimeISO($1)")>]
    abstract ToZonedDateTimeISO: timeZone: string -> ZonedDateTime

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: InstantFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: InstantLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: InstantLocaleOptions -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: InstantStyleOptions * ?locales: string -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: InstantStyleOptions * locales: string array -> string


/// An immutable calendar date without a clock time or time zone.
and PlainDate =
    [<Emit("$0.era")>]
    abstract Era: string option

    [<Emit("$0.eraYear")>]
    abstract EraYear: int option

    [<Emit("$0.year")>]
    abstract Year: int

    [<Emit("$0.month")>]
    abstract Month: int

    [<Emit("$0.monthCode")>]
    abstract MonthCode: string

    [<Emit("$0.calendarId")>]
    abstract CalendarId: string

    [<Emit("$0.daysInMonth")>]
    abstract DaysInMonth: int

    [<Emit("$0.daysInYear")>]
    abstract DaysInYear: int

    [<Emit("$0.monthsInYear")>]
    abstract MonthsInYear: int

    [<Emit("$0.inLeapYear")>]
    abstract InLeapYear: bool

    [<Emit("$0.day")>]
    abstract Day: int

    [<Emit("$0.dayOfWeek")>]
    abstract DayOfWeek: int

    [<Emit("$0.dayOfYear")>]
    abstract DayOfYear: int

    [<Emit("$0.weekOfYear")>]
    abstract WeekOfYear: int option

    [<Emit("$0.yearOfWeek")>]
    abstract YearOfWeek: int option

    [<Emit("$0.daysInWeek")>]
    abstract DaysInWeek: int

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: PlainDate -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: DateFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: DatePatch * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: Duration * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: string * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: DurationFields * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: Duration * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: string * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: DurationFields * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: PlainDate * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: DateFields * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: PlainDate * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: DateFields * ?options: DateDifferenceOptions -> Duration

    [<Emit("$0.withCalendar($1)")>]
    abstract WithCalendar: calendar: string -> PlainDate

    [<Emit("$0.toPlainDateTime($1)")>]
    abstract ToPlainDateTime: ?time: PlainTime -> PlainDateTime

    [<Emit("$0.toZonedDateTime($1)")>]
    abstract ToZonedDateTime: timeZone: string -> ZonedDateTime

    [<Emit("$0.toZonedDateTime($1)")>]
    abstract ToZonedDateTime: options: DateToZonedOptions -> ZonedDateTime

    [<Emit("$0.toPlainYearMonth()")>]
    abstract ToPlainYearMonth: unit -> PlainYearMonth

    [<Emit("$0.toPlainMonthDay()")>]
    abstract ToPlainMonthDay: unit -> PlainMonthDay

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: CalendarFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: DateLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: DateLocaleOptions -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: DateStyleOptions * ?locales: string -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: DateStyleOptions * locales: string array -> string


/// An immutable wall-clock time with nanosecond precision and no date.
and PlainTime =
    [<Emit("$0.hour")>]
    abstract Hour: int

    [<Emit("$0.minute")>]
    abstract Minute: int

    [<Emit("$0.second")>]
    abstract Second: int

    [<Emit("$0.millisecond")>]
    abstract Millisecond: int

    [<Emit("$0.microsecond")>]
    abstract Microsecond: int

    [<Emit("$0.nanosecond")>]
    abstract Nanosecond: int

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: PlainTime -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: TimeFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: TimePatch * ?options: OverflowOptions -> PlainTime

    [<Emit("$0.add($1)")>]
    abstract Add: duration: Duration -> PlainTime

    [<Emit("$0.add($1)")>]
    abstract Add: duration: string -> PlainTime

    [<Emit("$0.add($1)")>]
    abstract Add: duration: DurationFields -> PlainTime

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: duration: Duration -> PlainTime

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: duration: string -> PlainTime

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: duration: DurationFields -> PlainTime

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: PlainTime * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: TimeFields * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: PlainTime * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: TimeFields * ?options: TimeDifferenceOptions -> Duration

    [<Emit("$0.round($1)")>]
    abstract Round: options: TimeRoundOptions -> PlainTime

    [<Emit("$0.round($1)")>]
    abstract Round: smallestUnit: TimeUnit -> PlainTime

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: PrecisionOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: TimeLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: TimeLocaleOptions -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: TimeStyleOptions * ?locales: string -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: TimeStyleOptions * locales: string array -> string


/// An immutable calendar date and clock time without a time zone.
and PlainDateTime =
    [<Emit("$0.era")>]
    abstract Era: string option

    [<Emit("$0.eraYear")>]
    abstract EraYear: int option

    [<Emit("$0.year")>]
    abstract Year: int

    [<Emit("$0.month")>]
    abstract Month: int

    [<Emit("$0.monthCode")>]
    abstract MonthCode: string

    [<Emit("$0.calendarId")>]
    abstract CalendarId: string

    [<Emit("$0.daysInMonth")>]
    abstract DaysInMonth: int

    [<Emit("$0.daysInYear")>]
    abstract DaysInYear: int

    [<Emit("$0.monthsInYear")>]
    abstract MonthsInYear: int

    [<Emit("$0.inLeapYear")>]
    abstract InLeapYear: bool

    [<Emit("$0.day")>]
    abstract Day: int

    [<Emit("$0.dayOfWeek")>]
    abstract DayOfWeek: int

    [<Emit("$0.dayOfYear")>]
    abstract DayOfYear: int

    [<Emit("$0.weekOfYear")>]
    abstract WeekOfYear: int option

    [<Emit("$0.yearOfWeek")>]
    abstract YearOfWeek: int option

    [<Emit("$0.daysInWeek")>]
    abstract DaysInWeek: int

    [<Emit("$0.hour")>]
    abstract Hour: int

    [<Emit("$0.minute")>]
    abstract Minute: int

    [<Emit("$0.second")>]
    abstract Second: int

    [<Emit("$0.millisecond")>]
    abstract Millisecond: int

    [<Emit("$0.microsecond")>]
    abstract Microsecond: int

    [<Emit("$0.nanosecond")>]
    abstract Nanosecond: int

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: PlainDateTime -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: DateTimeFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: DateTimePatch * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: Duration * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: string * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: DurationFields * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: Duration * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: string * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: DurationFields * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: PlainDateTime * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: DateTimeFields * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: PlainDateTime * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: DateTimeFields * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.round($1)")>]
    abstract Round: options: DayTimeRoundOptions -> PlainDateTime

    [<Emit("$0.round($1)")>]
    abstract Round: smallestUnit: DayTimeUnit -> PlainDateTime

    [<Emit("$0.withCalendar($1)")>]
    abstract WithCalendar: calendar: string -> PlainDateTime

    [<Emit("$0.withPlainTime($1)")>]
    abstract WithPlainTime: ?time: PlainTime -> PlainDateTime

    [<Emit("$0.toPlainDate()")>]
    abstract ToPlainDate: unit -> PlainDate

    [<Emit("$0.toPlainTime()")>]
    abstract ToPlainTime: unit -> PlainTime

    [<Emit("$0.toZonedDateTime($1, $2)")>]
    abstract ToZonedDateTime: timeZone: string * ?options: DisambiguationOptions -> ZonedDateTime

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: DateTimeFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: DateTimeLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: DateTimeLocaleOptions -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: DateTimeStyleOptions * ?locales: string -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: DateTimeStyleOptions * locales: string array -> string


/// An immutable calendar year and month, retaining Temporal reference-day semantics.
and PlainYearMonth =
    [<Emit("$0.era")>]
    abstract Era: string option

    [<Emit("$0.eraYear")>]
    abstract EraYear: int option

    [<Emit("$0.year")>]
    abstract Year: int

    [<Emit("$0.month")>]
    abstract Month: int

    [<Emit("$0.monthCode")>]
    abstract MonthCode: string

    [<Emit("$0.calendarId")>]
    abstract CalendarId: string

    [<Emit("$0.daysInMonth")>]
    abstract DaysInMonth: int

    [<Emit("$0.daysInYear")>]
    abstract DaysInYear: int

    [<Emit("$0.monthsInYear")>]
    abstract MonthsInYear: int

    [<Emit("$0.inLeapYear")>]
    abstract InLeapYear: bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: PlainYearMonth -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: YearMonthFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: YearMonthPatch * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: Duration * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: string * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: DurationFields * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: Duration * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: string * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: DurationFields * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: PlainYearMonth * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: YearMonthFields * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: PlainYearMonth * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: YearMonthFields * ?options: YearMonthDifferenceOptions -> Duration

    [<Emit("$0.toPlainDate($1)")>]
    abstract ToPlainDate: day: DayFields -> PlainDate

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: CalendarFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: YearMonthLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: YearMonthLocaleOptions -> string

/// An immutable recurring month/day, retaining Temporal reference-year semantics.
and PlainMonthDay =
    [<Emit("$0.monthCode")>]
    abstract MonthCode: string

    [<Emit("$0.day")>]
    abstract Day: int

    [<Emit("$0.calendarId")>]
    abstract CalendarId: string

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: PlainMonthDay -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: MonthDayFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: MonthDayPatch * ?options: OverflowOptions -> PlainMonthDay

    [<Emit("$0.toPlainDate($1)")>]
    abstract ToPlainDate: year: YearFields -> PlainDate

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: CalendarFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: MonthDayLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: MonthDayLocaleOptions -> string

/// An immutable instant carrying a time-zone identifier and calendar.
and ZonedDateTime =
    [<Emit("$0.era")>]
    abstract Era: string option

    [<Emit("$0.eraYear")>]
    abstract EraYear: int option

    [<Emit("$0.year")>]
    abstract Year: int

    [<Emit("$0.month")>]
    abstract Month: int

    [<Emit("$0.monthCode")>]
    abstract MonthCode: string

    [<Emit("$0.calendarId")>]
    abstract CalendarId: string

    [<Emit("$0.daysInMonth")>]
    abstract DaysInMonth: int

    [<Emit("$0.daysInYear")>]
    abstract DaysInYear: int

    [<Emit("$0.monthsInYear")>]
    abstract MonthsInYear: int

    [<Emit("$0.inLeapYear")>]
    abstract InLeapYear: bool

    [<Emit("$0.day")>]
    abstract Day: int

    [<Emit("$0.dayOfWeek")>]
    abstract DayOfWeek: int

    [<Emit("$0.dayOfYear")>]
    abstract DayOfYear: int

    [<Emit("$0.weekOfYear")>]
    abstract WeekOfYear: int option

    [<Emit("$0.yearOfWeek")>]
    abstract YearOfWeek: int option

    [<Emit("$0.daysInWeek")>]
    abstract DaysInWeek: int

    [<Emit("$0.hour")>]
    abstract Hour: int

    [<Emit("$0.minute")>]
    abstract Minute: int

    [<Emit("$0.second")>]
    abstract Second: int

    [<Emit("$0.millisecond")>]
    abstract Millisecond: int

    [<Emit("$0.microsecond")>]
    abstract Microsecond: int

    [<Emit("$0.nanosecond")>]
    abstract Nanosecond: int

    [<Emit("$0.epochMilliseconds")>]
    abstract EpochMilliseconds: float

    [<Emit("$0.epochNanoseconds")>]
    abstract EpochNanoseconds: bigint

    [<Emit("$0.timeZoneId")>]
    abstract TimeZoneId: string

    [<Emit("$0.offset")>]
    abstract Offset: string

    [<Emit("$0.offsetNanoseconds")>]
    abstract OffsetNanoseconds: float

    [<Emit("$0.hoursInDay")>]
    abstract HoursInDay: float

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: ZonedDateTime -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: string -> bool

    [<Emit("$0.equals($1)")>]
    abstract IsEqualTo: other: ZonedDateTimeFields -> bool

    [<Emit("$0.with($1, $2)")>]
    abstract With: fields: ZonedPatch * ?options: ZonedOptions -> ZonedDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: Duration * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: string * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.add($1, $2)")>]
    abstract Add: duration: DurationFields * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: Duration * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: string * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.subtract($1, $2)")>]
    abstract Subtract: duration: DurationFields * ?options: OverflowOptions -> ZonedDateTime

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: ZonedDateTime * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: string * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.until($1, $2)")>]
    abstract Until: other: ZonedDateTimeFields * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: ZonedDateTime * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: string * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.since($1, $2)")>]
    abstract Since: other: ZonedDateTimeFields * ?options: DateTimeDifferenceOptions -> Duration

    [<Emit("$0.round($1)")>]
    abstract Round: options: DayTimeRoundOptions -> ZonedDateTime

    [<Emit("$0.round($1)")>]
    abstract Round: smallestUnit: DayTimeUnit -> ZonedDateTime

    [<Emit("$0.withCalendar($1)")>]
    abstract WithCalendar: calendar: string -> ZonedDateTime

    [<Emit("$0.withPlainTime($1)")>]
    abstract WithPlainTime: ?time: PlainTime -> ZonedDateTime

    [<Emit("$0.toPlainDate()")>]
    abstract ToPlainDate: unit -> PlainDate

    [<Emit("$0.toPlainTime()")>]
    abstract ToPlainTime: unit -> PlainTime

    [<Emit("$0.withTimeZone($1)")>]
    abstract WithTimeZone: timeZone: string -> ZonedDateTime

    [<Emit("$0.startOfDay()")>]
    abstract StartOfDay: unit -> ZonedDateTime

    /// Return the next or previous offset transition, or None when none exists.
    [<Emit("$0.getTimeZoneTransition($1) ?? undefined")>]
    abstract GetTimeZoneTransition: direction: TransitionDirection -> ZonedDateTime option

    [<Emit("$0.toInstant()")>]
    abstract ToInstant: unit -> Instant

    [<Emit("$0.toPlainDateTime()")>]
    abstract ToPlainDateTime: unit -> PlainDateTime

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: ZonedFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: ZonedLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: ZonedLocaleOptions -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: ZonedStyleOptions * ?locales: string -> string

    [<Emit("$0.toLocaleString($2, $1)")>]
    abstract ToLocaleString: options: ZonedStyleOptions * locales: string array -> string


/// An immutable set of signed calendar and clock components. General totals and rounding require context.
and Duration =
    [<Emit("$0.years")>]
    abstract Years: float

    [<Emit("$0.months")>]
    abstract Months: float

    [<Emit("$0.weeks")>]
    abstract Weeks: float

    [<Emit("$0.days")>]
    abstract Days: float

    [<Emit("$0.hours")>]
    abstract Hours: float

    [<Emit("$0.minutes")>]
    abstract Minutes: float

    [<Emit("$0.seconds")>]
    abstract Seconds: float

    [<Emit("$0.milliseconds")>]
    abstract Milliseconds: float

    [<Emit("$0.microseconds")>]
    abstract Microseconds: float

    [<Emit("$0.nanoseconds")>]
    abstract Nanoseconds: float

    [<Emit("$0.sign")>]
    abstract Sign: int

    [<Emit("$0.blank")>]
    abstract Blank: bool

    /// Replace unsigned magnitudes using the receiver sign; zero receivers become positive.
    [<Emit("((duration, fields) => duration.with(Object.fromEntries(Object.entries(fields).map(([key, value]) => [key, value * (duration.sign || 1)]))))($0, $1)")>]
    abstract WithMagnitude: fields: DurationPatch -> Duration

    [<Emit("$0.negated()")>]
    abstract Negated: unit -> Duration

    [<Emit("$0.abs()")>]
    abstract Abs: unit -> Duration

    [<Emit("$0.round($1)")>]
    abstract Round: options: DurationRoundOptions -> Duration

    [<Emit("$0.total($1)")>]
    abstract Total: options: TotalOptions -> float

    [<Emit("$0.toString($1)")>]
    abstract ToString: ?options: DurationFormatOptions -> string

    [<Emit("$0.toJSON()")>]
    abstract ToJSON: unit -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: ?locales: string * ?options: DurationLocaleOptions -> string

    [<Emit("$0.toLocaleString($1, $2)")>]
    abstract ToLocaleString: locales: string array * ?options: DurationLocaleOptions -> string

/// A duration without years, months, or weeks. Context-free operations interpret days as 24 hours.
and FixedDuration =
    inherit Duration

    [<Emit("$0.add($1)")>]
    abstract Add: other: FixedDuration -> FixedDuration

    [<Emit("$0.add($1)")>]
    abstract Add: fields: FixedDurationFields -> FixedDuration

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: other: FixedDuration -> FixedDuration

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: fields: FixedDurationFields -> FixedDuration

    [<Emit("$0.round($1)")>]
    abstract Round: smallestUnit: DayTimeUnit -> FixedDuration

    [<Emit("$0.round($1)")>]
    abstract Round: options: DayTimeRoundOptions -> FixedDuration

    [<Emit("$0.total($1)")>]
    abstract Total: unit: DayTimeUnit -> float

/// A duration containing only clock components, suitable for instant addition and subtraction.
and TimeDuration =
    inherit FixedDuration

    [<Emit("$0.add($1)")>]
    abstract Add: other: TimeDuration -> TimeDuration

    [<Emit("$0.add($1)")>]
    abstract Add: fields: TimeDurationFields -> TimeDuration

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: other: TimeDuration -> TimeDuration

    [<Emit("$0.subtract($1)")>]
    abstract Subtract: fields: TimeDurationFields -> TimeDuration

/// Create, parse, or copy Instant values using the selected implementation.
and InstantFactory =
    [<Emit("$0.from($1)")>]
    abstract Parse: text: string -> Instant

    [<Emit("$0.from($1)")>]
    abstract From: value: Instant -> Instant

    [<Emit("new ($0)($1)")>]
    abstract Create: epochNanoseconds: bigint -> Instant

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: Instant * two: Instant -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.fromEpochNanoseconds($1)")>]
    abstract FromEpochNanoseconds: nanoseconds: bigint -> Instant

    [<Emit("$0.fromEpochMilliseconds($1)")>]
    abstract FromEpochMilliseconds: milliseconds: float -> Instant

/// Create, parse, or copy PlainDate values using the selected implementation.
and PlainDateFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: PlainDate * ?options: OverflowOptions -> PlainDate

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: DateFields * ?options: OverflowOptions -> PlainDate

    [<Emit("new ($0)($1, $2, $3, $4)")>]
    abstract Create: isoYear: int * isoMonth: int * isoDay: int * ?calendar: string -> PlainDate

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: PlainDate * two: PlainDate -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: DateFields * two: DateFields -> int

/// Create, parse, or copy PlainTime values using the selected implementation.
and PlainTimeFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: OverflowOptions -> PlainTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: PlainTime * ?options: OverflowOptions -> PlainTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: TimeFields * ?options: OverflowOptions -> PlainTime

    [<Emit("new ($0)($1, $2, $3, $4, $5, $6)")>]
    abstract Create:
        ?hour: int * ?minute: int * ?second: int * ?millisecond: int * ?microsecond: int * ?nanosecond: int -> PlainTime

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: PlainTime * two: PlainTime -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: TimeFields * two: TimeFields -> int

/// Create, parse, or copy PlainDateTime values using the selected implementation.
and PlainDateTimeFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: PlainDateTime * ?options: OverflowOptions -> PlainDateTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: DateTimeFields * ?options: OverflowOptions -> PlainDateTime

    [<Emit("new ($0)($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)")>]
    abstract Create:
        isoYear: int *
        isoMonth: int *
        isoDay: int *
        ?hour: int *
        ?minute: int *
        ?second: int *
        ?millisecond: int *
        ?microsecond: int *
        ?nanosecond: int *
        ?calendar: string ->
            PlainDateTime

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: PlainDateTime * two: PlainDateTime -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: DateTimeFields * two: DateTimeFields -> int

/// Create, parse, or copy PlainYearMonth values using the selected implementation.
and PlainYearMonthFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: PlainYearMonth * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: YearMonthFields * ?options: OverflowOptions -> PlainYearMonth

    [<Emit("new ($0)($1, $2, $3, $4)")>]
    abstract Create: isoYear: int * isoMonth: int * ?calendar: string * ?referenceISODay: int -> PlainYearMonth

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: PlainYearMonth * two: PlainYearMonth -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: YearMonthFields * two: YearMonthFields -> int

/// Create, parse, or copy PlainMonthDay values using the selected implementation.
and PlainMonthDayFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: OverflowOptions -> PlainMonthDay

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: PlainMonthDay * ?options: OverflowOptions -> PlainMonthDay

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: MonthDayFields * ?options: OverflowOptions -> PlainMonthDay

    [<Emit("new ($0)($1, $2, $3, $4)")>]
    abstract Create: isoMonth: int * isoDay: int * ?calendar: string * ?referenceISOYear: int -> PlainMonthDay

/// Create, parse, or copy ZonedDateTime values using the selected implementation.
and ZonedDateTimeFactory =
    [<Emit("$0.from($1, $2)")>]
    abstract Parse: text: string * ?options: ZonedOptions -> ZonedDateTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: ZonedDateTime * ?options: ZonedOptions -> ZonedDateTime

    [<Emit("$0.from($1, $2)")>]
    abstract From: value: ZonedDateTimeFields * ?options: ZonedOptions -> ZonedDateTime

    [<Emit("new ($0)($1, $2, $3)")>]
    abstract Create: epochNanoseconds: bigint * timeZone: string * ?calendar: string -> ZonedDateTime

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: ZonedDateTime * two: ZonedDateTime -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: string * two: string -> int

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: ZonedDateTimeFields * two: ZonedDateTimeFields -> int

/// Create, parse, or copy Duration values using the selected implementation.
and DurationFactory =
    [<Emit("$0.from($1)")>]
    abstract Parse: text: string -> Duration

    [<Emit("$0.from($1)")>]
    abstract From: value: Duration -> Duration

    [<Emit("$0.from($1)")>]
    abstract From: value: DurationFields -> Duration

    [<Emit("new ($0)($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)")>]
    abstract Create:
        ?years: uint32 *
        ?months: uint32 *
        ?weeks: uint32 *
        ?days: uint32 *
        ?hours: uint32 *
        ?minutes: uint32 *
        ?seconds: uint32 *
        ?milliseconds: uint32 *
        ?microseconds: uint32 *
        ?nanoseconds: uint32 ->
            Duration

    [<Emit("$0.compare($1, $2, $3)")>]
    abstract Compare: one: Duration * two: Duration * options: RelativeToOptions -> int

    [<Emit("$0.compare($1, $2, $3)")>]
    abstract Compare: one: string * two: string * options: RelativeToOptions -> int

    [<Emit("$0.compare($1, $2, $3)")>]
    abstract Compare: one: DurationFields * two: DurationFields * options: RelativeToOptions -> int

    [<Emit("new ($0)(0, 0, 0, 0, $1, $2, $3, $4, $5, $6)")>]
    abstract CreateTime:
        ?hours: uint32 *
        ?minutes: uint32 *
        ?seconds: uint32 *
        ?milliseconds: uint32 *
        ?microseconds: uint32 *
        ?nanoseconds: uint32 ->
            TimeDuration

    [<Emit("new ($0)(0, 0, 0, $1, $2, $3, $4, $5, $6, $7)")>]
    abstract CreateFixed:
        ?days: uint32 *
        ?hours: uint32 *
        ?minutes: uint32 *
        ?seconds: uint32 *
        ?milliseconds: uint32 *
        ?microseconds: uint32 *
        ?nanoseconds: uint32 ->
            FixedDuration

    [<Emit("$0.compare($1, $2)")>]
    abstract Compare: one: FixedDuration * two: FixedDuration -> int


/// Read the selected implementation clock and system time-zone identifier.
and TemporalNow =
    [<Emit("$0.instant()")>]
    abstract Instant: unit -> Instant

    [<Emit("$0.timeZoneId()")>]
    abstract TimeZoneId: unit -> string

    [<Emit("$0.zonedDateTimeISO($1)")>]
    abstract ZonedDateTimeISO: ?timeZone: string -> ZonedDateTime

    [<Emit("$0.plainDateTimeISO($1)")>]
    abstract PlainDateTimeISO: ?timeZone: string -> PlainDateTime

    [<Emit("$0.plainDateISO($1)")>]
    abstract PlainDateISO: ?timeZone: string -> PlainDate

    [<Emit("$0.plainTimeISO($1)")>]
    abstract PlainTimeISO: ?timeZone: string -> PlainTime

/// The Temporal namespace supplied by an application-selected native engine or polyfill.
and TemporalImplementation =
    [<Emit("$0.Instant")>]
    abstract Instant: InstantFactory

    [<Emit("$0.PlainDate")>]
    abstract PlainDate: PlainDateFactory

    [<Emit("$0.PlainTime")>]
    abstract PlainTime: PlainTimeFactory

    [<Emit("$0.PlainDateTime")>]
    abstract PlainDateTime: PlainDateTimeFactory

    [<Emit("$0.PlainYearMonth")>]
    abstract PlainYearMonth: PlainYearMonthFactory

    [<Emit("$0.PlainMonthDay")>]
    abstract PlainMonthDay: PlainMonthDayFactory

    [<Emit("$0.ZonedDateTime")>]
    abstract ZonedDateTime: ZonedDateTimeFactory

    [<Emit("$0.Duration")>]
    abstract Duration: DurationFactory

    [<Emit("$0.Now")>]
    abstract Now: TemporalNow

/// Hold and check an implementation without modifying globals; direct namespace imports avoid this wrapper.
type TemporalRuntime(implementation: TemporalImplementation) =
    do
        if isNull (box implementation) then
            invalidArg
                "implementation"
                "Temporal is unavailable. Supply a Temporal polyfill or use a runtime with native Temporal."

    member _.Instant = implementation.Instant
    member _.PlainDate = implementation.PlainDate
    member _.PlainTime = implementation.PlainTime
    member _.PlainDateTime = implementation.PlainDateTime
    member _.PlainYearMonth = implementation.PlainYearMonth
    member _.PlainMonthDay = implementation.PlainMonthDay
    member _.ZonedDateTime = implementation.ZonedDateTime
    member _.Duration = implementation.Duration
    member _.Now = implementation.Now

    /// Look up globalThis.Temporal now and throw if it is absent.
    static member Native =
        let implementation: TemporalImplementation = emitJsExpr () "globalThis.Temporal"
        TemporalRuntime(implementation)

    /// Test for a global Temporal object, including one installed by an application polyfill.
    static member IsNativeAvailable: bool =
        emitJsExpr () "typeof globalThis.Temporal !== 'undefined'"

/// Classify a caught JavaScript value by error name while retaining the original cause.
[<RequireQualifiedAccess>]
type TemporalError =
    | RangeError of exn
    | TypeError of exn
    | Other of exn

    /// The exact caught value, without replacing its identity, message, or stack.
    member this.Cause =
        match this with
        | TemporalError.RangeError error
        | TemporalError.TypeError error
        | TemporalError.Other error -> error

[<RequireQualifiedAccess>]
module Temporal =
    /// Capture a synchronous callback as a Result, classifying errors by name. Does not await promises.
    let tryCall (operation: unit -> 'T) : Result<'T, TemporalError> =
        try
            Ok(operation ())
        with error ->
            let name: string = emitJsExpr error "$0?.name"

            match name with
            | "RangeError" -> Error(TemporalError.RangeError error)
            | "TypeError" -> Error(TemporalError.TypeError error)
            | _ -> Error(TemporalError.Other error)

[<RequireQualifiedAccess>]
module Duration =
    /// Compare elapsed duration lengths at the supplied anchor, returning -1, 0, or 1.
    let inline compare (relativeTo: RelativeTo) (left: Duration) (right: Duration) : int =
        emitJsExpr (left, right, RelativeToOptions(relativeTo)) "$0.constructor.compare($0, $1, $2)"

    /// Check for absent years, months, and weeks; return the same object or an explanatory Error.
    let tryAsFixed (duration: Duration) : Result<FixedDuration, string> =
        if duration.Years = 0. && duration.Months = 0. && duration.Weeks = 0. then
            Ok(unbox<FixedDuration> duration)
        else
            Error "A fixed duration cannot contain years, months, or weeks."

    /// Check for absent date components before instant arithmetic; retain the object on success.
    let tryAsTime (duration: Duration) : Result<TimeDuration, string> =
        if
            duration.Years = 0.
            && duration.Months = 0.
            && duration.Weeks = 0.
            && duration.Days = 0.
        then
            Ok(unbox<TimeDuration> duration)
        else
            Error "A time duration cannot contain years, months, weeks, or days."

[<RequireQualifiedAccess>]
module Instant =
    /// Compare Temporal values for equality.
    let inline equals (left: Instant) (right: Instant) : bool = left.IsEqualTo(right)

    /// Order exact timeline points. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: Instant) (right: Instant) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

[<RequireQualifiedAccess>]
module PlainDate =
    /// Compare Temporal values for equality, including calendar identity.
    let inline equals (left: PlainDate) (right: PlainDate) : bool = left.IsEqualTo(right)

    /// Order ISO dates; calendar identity is ignored. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: PlainDate) (right: PlainDate) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

[<RequireQualifiedAccess>]
module PlainTime =
    /// Compare Temporal values for equality.
    let inline equals (left: PlainTime) (right: PlainTime) : bool = left.IsEqualTo(right)

    /// Order wall-clock times. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: PlainTime) (right: PlainTime) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

[<RequireQualifiedAccess>]
module PlainDateTime =
    /// Compare Temporal values for equality, including calendar identity.
    let inline equals (left: PlainDateTime) (right: PlainDateTime) : bool = left.IsEqualTo(right)

    /// Order ISO dates and clock times; calendar identity is ignored. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: PlainDateTime) (right: PlainDateTime) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

[<RequireQualifiedAccess>]
module PlainYearMonth =
    /// Compare Temporal values for equality, including calendar identity.
    let inline equals (left: PlainYearMonth) (right: PlainYearMonth) : bool = left.IsEqualTo(right)

    /// Order ISO reference dates; calendar identity is ignored. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: PlainYearMonth) (right: PlainYearMonth) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

[<RequireQualifiedAccess>]
module PlainMonthDay =
    /// Compare Temporal values for equality, including calendar identity.
    let inline equals (left: PlainMonthDay) (right: PlainMonthDay) : bool = left.IsEqualTo(right)

[<RequireQualifiedAccess>]
module ZonedDateTime =
    /// Compare Temporal values for equality, including zone and calendar identity.
    let inline equals (left: ZonedDateTime) (right: ZonedDateTime) : bool = left.IsEqualTo(right)

    /// Order timeline points; zone and calendar identity are ignored. Returns -1, 0, or 1 using the left value constructor.
    let inline compare (left: ZonedDateTime) (right: ZonedDateTime) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"


[<RequireQualifiedAccess>]
module FixedDuration =
    /// Compare elapsed duration lengths with 24-hour days, returning -1, 0, or 1.
    let inline compare (left: FixedDuration) (right: FixedDuration) : int =
        emitJsExpr (left, right) "$0.constructor.compare($0, $1)"

/// Optional Result-returning entry points for parsing, construction, and zone resolution.
[<AutoOpen>]
module TemporalTryExtensions =

    type Instant with

        /// Run ToZonedDateTimeISO and capture its error without changing the underlying operation defaults.
        member this.TryToZonedDateTimeISO(timeZone: string) : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.ToZonedDateTimeISO(timeZone))

    type PlainDate with

        /// Run ToZonedDateTime and capture its error without changing the underlying operation defaults.
        member this.TryToZonedDateTime(timeZone: string) : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.ToZonedDateTime(timeZone))

        /// Run ToZonedDateTime and capture its error without changing the underlying operation defaults.
        member this.TryToZonedDateTime(options: DateToZonedOptions) : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.ToZonedDateTime(options))

    type PlainDateTime with

        /// Run ToZonedDateTime and capture its error without changing the underlying operation defaults.
        member this.TryToZonedDateTime
            (timeZone: string, ?options: DisambiguationOptions)
            : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.ToZonedDateTime(timeZone, ?options = options))

    type InstantFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string) : Result<Instant, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate(epochNanoseconds: bigint) : Result<Instant, TemporalError> =
            Temporal.tryCall (fun () -> this.Create(epochNanoseconds))

        /// Run FromEpochNanoseconds and capture its error without changing the underlying operation defaults.
        member this.TryFromEpochNanoseconds(nanoseconds: bigint) : Result<Instant, TemporalError> =
            Temporal.tryCall (fun () -> this.FromEpochNanoseconds(nanoseconds))

        /// Run FromEpochMilliseconds and capture its error without changing the underlying operation defaults.
        member this.TryFromEpochMilliseconds(milliseconds: float) : Result<Instant, TemporalError> =
            Temporal.tryCall (fun () -> this.FromEpochMilliseconds(milliseconds))

    type PlainDateFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: OverflowOptions) : Result<PlainDate, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: DateFields, ?options: OverflowOptions) : Result<PlainDate, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (isoYear: int, isoMonth: int, isoDay: int, ?calendar: string)
            : Result<PlainDate, TemporalError> =
            Temporal.tryCall (fun () -> this.Create(isoYear, isoMonth, isoDay, ?calendar = calendar))

    type PlainTimeFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: OverflowOptions) : Result<PlainTime, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: TimeFields, ?options: OverflowOptions) : Result<PlainTime, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (?hour: int, ?minute: int, ?second: int, ?millisecond: int, ?microsecond: int, ?nanosecond: int)
            : Result<PlainTime, TemporalError> =
            Temporal.tryCall (fun () ->
                this.Create(
                    ?hour = hour,
                    ?minute = minute,
                    ?second = second,
                    ?millisecond = millisecond,
                    ?microsecond = microsecond,
                    ?nanosecond = nanosecond
                ))

    type PlainDateTimeFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: OverflowOptions) : Result<PlainDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: DateTimeFields, ?options: OverflowOptions) : Result<PlainDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (
                isoYear: int,
                isoMonth: int,
                isoDay: int,
                ?hour: int,
                ?minute: int,
                ?second: int,
                ?millisecond: int,
                ?microsecond: int,
                ?nanosecond: int,
                ?calendar: string
            ) : Result<PlainDateTime, TemporalError> =
            Temporal.tryCall (fun () ->
                this.Create(
                    isoYear,
                    isoMonth,
                    isoDay,
                    ?hour = hour,
                    ?minute = minute,
                    ?second = second,
                    ?millisecond = millisecond,
                    ?microsecond = microsecond,
                    ?nanosecond = nanosecond,
                    ?calendar = calendar
                ))

    type PlainYearMonthFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: OverflowOptions) : Result<PlainYearMonth, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: YearMonthFields, ?options: OverflowOptions) : Result<PlainYearMonth, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (isoYear: int, isoMonth: int, ?calendar: string, ?referenceISODay: int)
            : Result<PlainYearMonth, TemporalError> =
            Temporal.tryCall (fun () ->
                this.Create(isoYear, isoMonth, ?calendar = calendar, ?referenceISODay = referenceISODay))

    type PlainMonthDayFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: OverflowOptions) : Result<PlainMonthDay, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: MonthDayFields, ?options: OverflowOptions) : Result<PlainMonthDay, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (isoMonth: int, isoDay: int, ?calendar: string, ?referenceISOYear: int)
            : Result<PlainMonthDay, TemporalError> =
            Temporal.tryCall (fun () ->
                this.Create(isoMonth, isoDay, ?calendar = calendar, ?referenceISOYear = referenceISOYear))

    type ZonedDateTimeFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string, ?options: ZonedOptions) : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text, ?options = options))

        /// Construct from fields without changing overflow defaults; use Reject to disallow adjustment.
        member this.TryFrom(value: ZonedDateTimeFields, ?options: ZonedOptions) : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value, ?options = options))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (epochNanoseconds: bigint, timeZone: string, ?calendar: string)
            : Result<ZonedDateTime, TemporalError> =
            Temporal.tryCall (fun () -> this.Create(epochNanoseconds, timeZone, ?calendar = calendar))

    type DurationFactory with

        /// Parse text, returning the classified Temporal error on failure.
        member this.TryParse(text: string) : Result<Duration, TemporalError> =
            Temporal.tryCall (fun () -> this.Parse(text))

        /// Construct a duration from component magnitudes, capturing errors from the implementation.
        member this.TryFrom(value: DurationFields) : Result<Duration, TemporalError> =
            Temporal.tryCall (fun () -> this.From(value))

        /// Run Create and capture its error without changing the underlying operation defaults.
        member this.TryCreate
            (
                ?years: uint32,
                ?months: uint32,
                ?weeks: uint32,
                ?days: uint32,
                ?hours: uint32,
                ?minutes: uint32,
                ?seconds: uint32,
                ?milliseconds: uint32,
                ?microseconds: uint32,
                ?nanoseconds: uint32
            ) : Result<Duration, TemporalError> =
            Temporal.tryCall (fun () ->
                this.Create(
                    ?years = years,
                    ?months = months,
                    ?weeks = weeks,
                    ?days = days,
                    ?hours = hours,
                    ?minutes = minutes,
                    ?seconds = seconds,
                    ?milliseconds = milliseconds,
                    ?microseconds = microseconds,
                    ?nanoseconds = nanoseconds
                ))

        /// Run CreateTime and capture its error without changing the underlying operation defaults.
        member this.TryCreateTime
            (
                ?hours: uint32,
                ?minutes: uint32,
                ?seconds: uint32,
                ?milliseconds: uint32,
                ?microseconds: uint32,
                ?nanoseconds: uint32
            ) : Result<TimeDuration, TemporalError> =
            Temporal.tryCall (fun () ->
                this.CreateTime(
                    ?hours = hours,
                    ?minutes = minutes,
                    ?seconds = seconds,
                    ?milliseconds = milliseconds,
                    ?microseconds = microseconds,
                    ?nanoseconds = nanoseconds
                ))

        /// Run CreateFixed and capture its error without changing the underlying operation defaults.
        member this.TryCreateFixed
            (
                ?days: uint32,
                ?hours: uint32,
                ?minutes: uint32,
                ?seconds: uint32,
                ?milliseconds: uint32,
                ?microseconds: uint32,
                ?nanoseconds: uint32
            ) : Result<FixedDuration, TemporalError> =
            Temporal.tryCall (fun () ->
                this.CreateFixed(
                    ?days = days,
                    ?hours = hours,
                    ?minutes = minutes,
                    ?seconds = seconds,
                    ?milliseconds = milliseconds,
                    ?microseconds = microseconds,
                    ?nanoseconds = nanoseconds
                ))
