module Tests

open Fable.Core
open Fable.Core.JsInterop
open Fable.Temporal

let polyfill: TemporalImplementation =
    import "Temporal" "temporal-polyfill/full/implementation"

let mutable assertions = 0

let equal label expected actual =
    assertions <- assertions + 1

    if expected <> actual then
        failwithf "%s: expected %A, got %A" label expected actual

let rangeError label action =
    let mutable caught = false

    try
        action ()
    with error ->
        caught <- true
        let name: string = emitJsExpr error "$0.name"
        equal (label + " exception") "RangeError" name

    equal label true caught

let value =
    function
    | Ok value -> value
    | Error error -> failwith error

let run name (t: TemporalRuntime) =
    let instant = t.Instant.Parse("2024-01-01T00:00:00.123456789Z")
    equal "epoch bigint" 1704067200123456789I instant.EpochNanoseconds
    equal "milliseconds" 1704067200123. instant.EpochMilliseconds
    equal "negative epoch" -1I (t.Instant.FromEpochNanoseconds(-1I)).EpochNanoseconds
    equal "nanosecond constructor" 1704067200123456789I (t.Instant.Create(1704067200123456789I)).EpochNanoseconds
    equal "millisecond factory" 1000000I (t.Instant.FromEpochMilliseconds(1.)).EpochNanoseconds
    equal "instant clone" true (t.Instant.From(instant).IsEqualTo(instant))

    equal
        "instant compare"
        -1
        (t.Instant.Compare(instant, instant.Add(TimeDurationFields(hours = 0u, nanoseconds = 1u))))

    equal
        "instant add"
        1704067200123456790I
        (instant.Add(TimeDurationFields(hours = 0u, nanoseconds = 1u))).EpochNanoseconds

    equal
        "instant subtract"
        1704067200123456788I
        (instant.Subtract(t.Duration.CreateTime(nanoseconds = 1u))).EpochNanoseconds

    equal "instant difference" 1. (instant.Until(instant.Add(t.Duration.CreateTime(seconds = 1u)))).Seconds
    equal "instant since" 1. (instant.Since(instant.Subtract(t.Duration.CreateTime(seconds = 1u)))).Seconds

    equal
        "fixed precision"
        "2024-01-01T00:00:00.123456789Z"
        (instant.ToString(
            InstantFormatOptions(fractionalSecondDigits = FractionalSecondDigits.Digits FractionalDigits.Nine)
        ))

    equal
        "format time zone"
        "2024-01-01T11:00:00.123456789+11:00"
        (instant.ToString(InstantFormatOptions(timeZone = "Australia/Melbourne")))

    equal "instant JSON" "2024-01-01T00:00:00.123456789Z" (instant.ToJSON())

    let rounded =
        instant.Round(TimeRoundOptions(TimeUnit.Millisecond, roundingMode = RoundingMode.Floor))

    equal "round precision" "2024-01-01T00:00:00.123Z" (rounded.ToString())
    equal "instant round unit" 1704067200000000000I (instant.Round(TimeUnit.Second)).EpochNanoseconds
    equal "instant zone" "Australia/Melbourne" (instant.ToZonedDateTimeISO("Australia/Melbourne")).TimeZoneId
    let max = t.Instant.FromEpochNanoseconds(8640000000000000000000I)
    equal "full Temporal epoch range" 8640000000000000000000I max.EpochNanoseconds
    rangeError "epoch out of range" (fun () -> t.Instant.FromEpochNanoseconds(8640000000000000000001I) |> ignore)

    let leap =
        t.PlainDate.From(DateFields(year = 2024, month = 2, day = 29), OverflowOptions(overflow = Overflow.Reject))

    equal "date constructor" "2024-02-29" ((t.PlainDate.Create(2024, 2, 29)).ToJSON())
    equal "leap year" true leap.InLeapYear
    equal "days in month" 29 leap.DaysInMonth
    equal "ISO week" (Some 9) leap.WeekOfYear
    equal "ISO week year" (Some 2024) leap.YearOfWeek
    equal "absent era" None leap.Era
    equal "month code" "M02" leap.MonthCode

    equal
        "constrain fields"
        "2023-02-28"
        ((t.PlainDate.From(DateFields(year = 2023, month = 2, day = 31), OverflowOptions(overflow = Overflow.Constrain)))
            .ToJSON())

    rangeError "reject fields" (fun () ->
        t.PlainDate.From(DateFields(year = 2023, month = 2, day = 31), OverflowOptions(overflow = Overflow.Reject))
        |> ignore)

    equal
        "constrain arithmetic"
        "2025-02-28"
        ((leap.Add(DurationFields(seconds = 0u, years = 1u), OverflowOptions(overflow = Overflow.Constrain))).ToJSON())

    rangeError "reject arithmetic" (fun () ->
        leap.Add(DurationFields(seconds = 0u, years = 1u), OverflowOptions(overflow = Overflow.Reject))
        |> ignore)

    equal "immutable with" "2024-02-20" ((leap.With(DatePatch.Create(DateChange.Day 20))).ToJSON())
    equal "original unchanged" 29 leap.Day
    equal "date subtract" "2024-02-28" ((leap.Subtract("P1D")).ToJSON())
    equal "date difference" 2. (leap.Until("2024-03-02")).Days
    equal "date since" 2. (leap.Since("2024-02-27")).Days
    equal "date compare" 0 (t.PlainDate.Compare(leap, t.PlainDate.Parse("2024-02-29")))
    equal "date equals fields" true (leap.IsEqualTo(DateFields(year = 2024, month = 2, day = 29)))
    equal "calendar conversion" "hebrew" (leap.WithCalendar("hebrew")).CalendarId
    equal "calendar roundtrip" "2024-02-29" ((leap.WithCalendar("hebrew").WithCalendar("iso8601")).ToJSON())

    equal
        "calendar annotation"
        "2024-02-29[u-ca=iso8601]"
        (leap.ToString(CalendarFormatOptions(calendarName = CalendarName.Always)))

    equal
        "calendar critical"
        "2024-02-29[!u-ca=iso8601]"
        (leap.ToString(CalendarFormatOptions(calendarName = CalendarName.Critical)))

    equal "year month conversion" "2024-02" ((leap.ToPlainYearMonth()).ToJSON())
    equal "month day conversion" "02-29" ((leap.ToPlainMonthDay()).ToJSON())
    equal "date midnight" 0 (leap.ToPlainDateTime()).Hour
    equal "date zone midnight" 0 (leap.ToZonedDateTime("UTC")).Hour

    equal
        "date zone time"
        12
        (leap.ToZonedDateTime(DateToZonedOptions("UTC", plainTime = t.PlainTime.Create(hour = 12)))).Hour

    let time =
        t.PlainTime.From(
            TimeFields(hour = 12, minute = 34, second = 56, millisecond = 123, microsecond = 456, nanosecond = 789)
        )

    equal "time precision" "12:34:56.123456789" (time.ToJSON())
    equal "nanoseconds field" 789 time.Nanosecond
    equal "microseconds field" 456 time.Microsecond
    equal "time constructor" "12:34:56.123456789" ((t.PlainTime.Create(12, 34, 56, 123, 456, 789)).ToJSON())
    equal "time wrap" "00:30:00" ((t.PlainTime.Parse("23:30").Add(DurationFields(seconds = 0u, hours = 1u))).ToJSON())
    equal "time subtract" "11:34:56.123456789" ((time.Subtract("PT1H")).ToJSON())
    equal "time with" 5 (time.With(TimePatch.Create(TimeChange.Hour 5))).Hour

    equal
        "time rounding"
        "12:30:00"
        ((time.Round(TimeRoundOptions(TimeUnit.Minute, roundingIncrement = 15, roundingMode = RoundingMode.HalfEven)))
            .ToJSON())

    equal "time compare" 0 (t.PlainTime.Compare(time, time))
    equal "time equals" true (time.IsEqualTo("12:34:56.123456789"))
    equal "time difference" 1. (time.Until(time.Add("PT1S"))).Seconds

    for mode, expected in
        [
            RoundingMode.Ceil, "12:35:00"
            RoundingMode.Floor, "12:34:00"
            RoundingMode.Expand, "12:35:00"
            RoundingMode.Trunc, "12:34:00"
            RoundingMode.HalfCeil, "12:35:00"
            RoundingMode.HalfFloor, "12:34:00"
            RoundingMode.HalfExpand, "12:35:00"
            RoundingMode.HalfTrunc, "12:34:00"
            RoundingMode.HalfEven, "12:34:00"
        ] do
        equal
            ("round mode " + string mode)
            expected
            ((t.PlainTime.Parse("12:34:30").Round(TimeRoundOptions(TimeUnit.Minute, roundingMode = mode))).ToJSON())

    let dt = leap.ToPlainDateTime(time)
    equal "date time precision" "2024-02-29T12:34:56.123456789" (dt.ToJSON())

    equal
        "date time constructor"
        "2024-02-29T12:34:56.123456789"
        ((t.PlainDateTime.Create(2024, 2, 29, 12, 34, 56, 123, 456, 789)).ToJSON())

    equal "date time fields" 1 (t.PlainDateTime.From(DateTimeFields(year = 2024, month = 1, day = 1))).Month
    equal "date time parse" true (dt.IsEqualTo(t.PlainDateTime.Parse(dt.ToJSON())))
    equal "date time compare" 0 (t.PlainDateTime.Compare(dt, dt))
    equal "date time with" 1 (dt.With(DateTimePatch.Create(DateTimeChange.Day 1))).Day
    equal "date time add" "2024-03-01T12:34:56.123456789" ((dt.Add("P1D")).ToJSON())
    equal "date time subtract" "2024-02-28T12:34:56.123456789" ((dt.Subtract("P1D")).ToJSON())
    equal "date time to time" true (dt.ToPlainTime().IsEqualTo(time))
    equal "date time to date" true (dt.ToPlainDate().IsEqualTo(leap))
    equal "date time with time" 0 (dt.WithPlainTime()).Hour
    equal "date time calendar" "gregory" (dt.WithCalendar("gregory")).CalendarId
    equal "date time round" "2024-03-01T00:00:00" ((dt.Round(DayTimeUnit.Day)).ToJSON())
    equal "date time difference" 1. (dt.Until(dt.Add("P1D"))).Days

    let ym = t.PlainYearMonth.Parse("2024-02")
    equal "year month constructor" true (t.PlainYearMonth.Create(2024, 2).IsEqualTo(ym))
    equal "year month fields" true (t.PlainYearMonth.From(YearMonthFields(year = 2024, month = 2)).IsEqualTo(ym))
    equal "year month add" "2024-03" ((ym.Add("P1M")).ToJSON())
    equal "year month subtract" "2024-01" ((ym.Subtract("P1M")).ToJSON())
    equal "year month with" 3 (ym.With(YearMonthPatch.Create(YearMonthChange.Month 3))).Month

    equal
        "year month difference"
        2.
        (ym.Until("2024-04", YearMonthDifferenceOptions(largestUnit = YearMonthUnit.Month))).Months

    equal "year month to date" "2024-02-29" ((ym.ToPlainDate(DayFields(29))).ToJSON())
    equal "year month compare" 0 (t.PlainYearMonth.Compare(ym, ym))
    let md = t.PlainMonthDay.Parse("02-29")
    equal "month day constructor" true (t.PlainMonthDay.Create(2, 29).IsEqualTo(md))
    equal "month day fields" true (t.PlainMonthDay.From(MonthDayFields(monthCode = "M02", day = 29)).IsEqualTo(md))
    equal "month day with" 28 (md.With(MonthDayPatch.Create(MonthDayChange.Day 28))).Day
    equal "month day to date" "2024-02-29" ((md.ToPlainDate(YearFields(2024))).ToJSON())

    let overlap = t.PlainDateTime.Parse("2024-04-07T02:30")

    let earlier =
        overlap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Earlier))

    let later =
        overlap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Later))

    equal "overlap earlier offset" "+11:00" earlier.Offset
    equal "overlap later offset" "+10:00" later.Offset
    equal "overlap one hour" 3600000000000I (later.EpochNanoseconds - earlier.EpochNanoseconds)

    equal
        "overlap compatible"
        earlier.EpochNanoseconds
        (overlap.ToZonedDateTime(
            "Australia/Melbourne",
            DisambiguationOptions(disambiguation = Disambiguation.Compatible)
        ))
            .EpochNanoseconds

    rangeError "overlap reject" (fun () ->
        overlap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Reject))
        |> ignore)

    let gap = t.PlainDateTime.Parse("2024-10-06T02:30")

    equal
        "gap earlier"
        1
        (gap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Earlier)))
            .Hour

    equal
        "gap later"
        3
        (gap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Later))).Hour

    equal
        "gap compatible"
        3
        (gap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Compatible)))
            .Hour

    rangeError "gap reject" (fun () ->
        gap.ToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Reject))
        |> ignore)

    let before = t.ZonedDateTime.Parse("2024-10-05T12:00+10:00[Australia/Melbourne]")
    equal "calendar day arithmetic" 12 (before.Add(DurationFields(seconds = 0u, days = 1u))).Hour
    equal "24 hour arithmetic" 13 (before.Add(DurationFields(seconds = 0u, hours = 24u))).Hour
    equal "23 hour day" 23. (before.Add("P1D")).HoursInDay
    equal "25 hour day" 25. earlier.HoursInDay

    equal
        "zoned constructor"
        before.EpochNanoseconds
        (t.ZonedDateTime.Create(before.EpochNanoseconds, "Australia/Melbourne")).EpochNanoseconds

    equal
        "zoned fields"
        before.EpochNanoseconds
        (t.ZonedDateTime.From(
            ZonedDateTimeFields(year = 2024, month = 10, day = 5, hour = 12, timeZone = "Australia/Melbourne")
        ))
            .EpochNanoseconds

    equal "zoned with" 11 (before.With(ZonedPatch.Create(ZonedChange.Hour 11))).Hour
    equal "zoned with time" 0 (before.WithPlainTime()).Hour
    equal "zoned with calendar" "gregory" (before.WithCalendar("gregory")).CalendarId
    equal "zoned subtract" 4 (before.Subtract("P1D")).Day
    equal "zoned compare" 0 (t.ZonedDateTime.Compare(before, before))
    equal "zoned equal" true (before.IsEqualTo(before.ToJSON()))
    equal "zone conversion preserves instant" before.EpochNanoseconds (before.WithTimeZone("UTC")).EpochNanoseconds
    equal "zoned to instant" before.EpochNanoseconds (before.ToInstant()).EpochNanoseconds
    equal "zoned to plain" 12 (before.ToPlainDateTime()).Hour
    equal "zoned to date" 5 (before.ToPlainDate()).Day
    equal "zoned to time" 12 (before.ToPlainTime()).Hour
    equal "start of day" 0 (before.StartOfDay()).Hour
    equal "offset nanos" 36000000000000. before.OffsetNanoseconds
    equal "zoned round" 12 (before.Round(DayTimeUnit.Hour)).Hour

    equal
        "zoned difference hours"
        23.
        (before.Until(before.Add("P1D"), DateTimeDifferenceOptions(largestUnit = DateTimeUnit.Hour))).Hours

    equal
        "zoned since"
        23.
        (before.Add("P1D").Since(before, DateTimeDifferenceOptions(largestUnit = DateTimeUnit.Hour))).Hours

    let transition = before.GetTimeZoneTransition(TransitionDirection.Next)
    equal "next transition exists" true transition.IsSome
    equal "next transition offset" "+11:00" transition.Value.Offset
    equal "previous transition" true (before.GetTimeZoneTransition(TransitionDirection.Previous)).IsSome
    equal "no UTC transitions" None (before.WithTimeZone("UTC").GetTimeZoneTransition(TransitionDirection.Next))

    equal
        "zoned format"
        "2024-10-05T12:00:00"
        (before.ToString(ZonedFormatOptions(timeZoneName = TimeZoneName.Never, offset = OffsetDisplay.Never)))

    let mismatch = "2024-04-07T02:30+09:00[Australia/Melbourne]"

    rangeError "offset reject" (fun () ->
        t.ZonedDateTime.Parse(mismatch, ZonedOptions(offset = OffsetPolicy.Reject))
        |> ignore)

    equal "offset ignore" "+11:00" (t.ZonedDateTime.Parse(mismatch, ZonedOptions(offset = OffsetPolicy.Ignore))).Offset
    equal "offset prefer" "+11:00" (t.ZonedDateTime.Parse(mismatch, ZonedOptions(offset = OffsetPolicy.Prefer))).Offset

    equal
        "offset use preserves instant"
        (t.Instant.Parse("2024-04-07T02:30+09:00")).EpochNanoseconds
        (t.ZonedDateTime.Parse(mismatch, ZonedOptions(offset = OffsetPolicy.Use))).EpochNanoseconds

    let duration = t.Duration.Parse("P1DT2H3M4.123456789S")
    equal "duration nano field" 789. duration.Nanoseconds
    equal "duration micro field" 456. duration.Microseconds
    equal "duration sign" 1 duration.Sign
    equal "duration blank" false duration.Blank
    equal "duration constructor" "PT1.000000001S" ((t.Duration.Create(seconds = 1u, nanoseconds = 1u)).ToJSON())

    equal
        "duration fields"
        "PT1.000000001S"
        ((t.Duration.From(DurationFields(seconds = 1u, nanoseconds = 1u))).ToJSON())

    equal "duration abs" 1 (duration.Negated().Abs()).Sign
    equal "duration negated" -1 (duration.Negated()).Sign
    equal "duration with" 5. (duration.WithMagnitude(DurationPatch.Create(DurationChange.Hours 5u))).Hours
    equal "duration add" "PT3H" ((t.Duration.CreateTime(hours = 1u).Add(t.Duration.CreateTime(hours = 2u))).ToJSON())

    equal
        "duration subtract"
        "PT1H"
        ((t.Duration.CreateTime(hours = 3u).Subtract(TimeDurationFields(hours = 2u))).ToJSON())

    equal "duration total" 90. (t.Duration.CreateTime(hours = 1u, minutes = 30u).Total(DayTimeUnit.Minute))

    equal
        "duration round"
        "PT2H"
        ((t.Duration
            .Parse("PT1H30M")
            .Round(
                DurationRoundOptions(
                    DateTimeUnit.Hour,
                    RelativeTo.Text "2024-01-01",
                    roundingMode = RoundingMode.HalfEven
                )
            ))
            .ToJSON())

    equal
        "duration compare"
        -1
        (t.Duration.Compare(t.Duration.CreateTime(hours = 1u), t.Duration.CreateTime(hours = 2u)))

    equal
        "duration relative total DST"
        23.
        (t.Duration.Parse("P1D").Total(TotalOptions(DateTimeUnit.Hour, relativeTo = RelativeTo.ZonedDateTime before)))

    equal
        "duration relative calendar total"
        29.
        (t.Duration
            .Parse("P1M")
            .Total(TotalOptions(DateTimeUnit.Day, relativeTo = RelativeTo.Date(t.PlainDate.Parse("2024-02-01")))))

    equal
        "duration relative comparison"
        -1
        (t.Duration.Compare("P1D", "PT24H", RelativeToOptions(relativeTo = RelativeTo.ZonedDateTime before)))


    equal
        "locale formatting"
        "29/02/2024"
        (leap.ToLocaleString(
            "en-GB",
            DateLocaleOptions(year = NumericStyle.Numeric, month = MonthStyle.TwoDigit, day = NumericStyle.TwoDigit)
        ))

    equal "locale list" "29/02/2024" (leap.ToLocaleString([| "en-GB" |]))
    equal "now zone id" true ((t.Now.TimeZoneId()).Length > 0)
    equal "now instant" true ((t.Now.Instant()).EpochNanoseconds > 0I)
    equal "now zoned" "UTC" (t.Now.ZonedDateTimeISO("UTC")).TimeZoneId
    equal "now date" "iso8601" (t.Now.PlainDateISO("UTC")).CalendarId
    equal "now datetime" "iso8601" (t.Now.PlainDateTimeISO("UTC")).CalendarId
    equal "now time" true ((t.Now.PlainTimeISO("UTC")).Hour >= 0)

    match t.Instant.TryParse("invalid") with
    | Error(TemporalError.RangeError error) ->
        equal "TryParse original error" "RangeError" (emitJsExpr error "$0.name": string)
    | Error _ -> failwith "TryParse should classify RangeError"
    | Ok _ -> failwith "TryParse should return Error"

    match t.PlainDate.TryParse("2024-02-29") with
    | Ok date -> equal "TryParse success" 29 date.Day
    | Error error -> raise error.Cause

    equal
        "instant supports increments beyond one minute"
        "2024-01-01T00:00:00Z"
        ((instant.Round(TimeRoundOptions(TimeUnit.Second, roundingIncrement = 120, roundingMode = RoundingMode.Floor)))
            .ToJSON())

    equal
        "relative date-time"
        24.
        (t.Duration
            .Parse("P1D")
            .Total(TotalOptions(DateTimeUnit.Hour, RelativeTo.DateTime(t.PlainDateTime.Parse("2024-01-01T12:00")))))

    equal
        "locale array with style"
        "29/02/2024"
        (leap.ToLocaleString(DateStyleOptions(dateStyle = FormatStyle.Short), locales = [| "en-GB" |]))

    let options = TimeRoundOptions(TimeUnit.Minute, roundingIncrement = 15)

    equal
        "option emits plain object"
        """{"smallestUnit":"minute","roundingIncrement":15}"""
        (emitJsExpr options "JSON.stringify($0)": string)

    equal "relative anchor is not wrapped" true (emitJsExpr (RelativeTo.ZonedDateTime before, before) "$0 === $1": bool)

    for increment in [ 0; -1; 7; 60; 1000000001 ] do
        rangeError "invalid clock increment" (fun () ->
            time.Round(TimeRoundOptions(TimeUnit.Minute, roundingIncrement = increment))
            |> ignore)

    rangeError "invalid unit ordering" (fun () ->
        leap.Until("2025-01-01", DateDifferenceOptions(smallestUnit = DateUnit.Year, largestUnit = DateUnit.Day))
        |> ignore)

    let matchingCalendarIncrement =
        DurationRoundOptions(
            DateTimeUnit.Day,
            RelativeTo.Text "2024-01-01",
            largestUnit = DateTimeUnit.Day,
            roundingIncrement = 2
        )

    equal "matching calendar increment" 4. (t.Duration.Parse("P3D").Round(matchingCalendarIncrement)).Days

    for increment in [ 1; 2; 3; 4; 5; 6; 10; 12; 15; 20; 30 ] do
        let options = TimeRoundOptions(TimeUnit.Minute, roundingIncrement = increment)

        equal "valid minute increment" true ((time.Round(options)).Minute >= 0)

    for increment in [ 1; 2; 3; 4; 6; 8; 12 ] do
        let options = TimeRoundOptions(TimeUnit.Hour, roundingIncrement = increment)

        equal "valid hour increment" true ((time.Round(options)).Hour >= 0)

    for increment in [ 1; 2; 4; 5; 8; 10; 20; 25; 40; 50; 100; 125; 200; 250; 500 ] do
        let options = TimeRoundOptions(TimeUnit.Nanosecond, roundingIncrement = increment)

        equal "valid nanosecond increment" true ((time.Round(options)).Nanosecond >= 0)

    let ordered =
        TimeDifferenceOptions(smallestUnit = TimeUnit.Second, largestUnit = TimeUnit.Minute)

    equal
        "valid difference ordering"
        1.
        (instant.Until(instant.Add(t.Duration.CreateTime(seconds = 90u)), ordered)).Minutes

    equal
        "default difference options"
        1.
        (instant.Until(instant.Add(t.Duration.CreateTime(seconds = 1u)), TimeDifferenceOptions())).Seconds

    let dateIncrement =
        DateDifferenceOptions(smallestUnit = DateUnit.Day, largestUnit = DateUnit.Day, roundingIncrement = 2)

    equal "date difference increment" 2. (leap.Until("2024-03-03", dateIncrement)).Days

    equal
        "duration seconds precision"
        "P1DT2H3M4S"
        (duration.ToString(DurationFormatOptions(smallestUnit = DurationPrecisionUnit.Second)))

    equal
        "auto precision"
        (instant.ToJSON())
        (instant.ToString(InstantFormatOptions(fractionalSecondDigits = FractionalSecondDigits.Auto)))

    for digits, suffix in
        [
            FractionalDigits.Zero, ""
            FractionalDigits.One, ".1"
            FractionalDigits.Two, ".12"
            FractionalDigits.Three, ".123"
            FractionalDigits.Four, ".1234"
            FractionalDigits.Five, ".12345"
            FractionalDigits.Six, ".123456"
            FractionalDigits.Seven, ".1234567"
            FractionalDigits.Eight, ".12345678"
            FractionalDigits.Nine, ".123456789"
        ] do
        equal
            "bounded fractional precision"
            ("2024-01-01T00:00:00" + suffix + "Z")
            (instant.ToString(InstantFormatOptions(fractionalSecondDigits = FractionalSecondDigits.Digits digits)))

    equal
        "multiple date patch fields"
        "2025-03-20"
        ((leap.With(DatePatch.Create(DateChange.Day 20, DateChange.Month 3, DateChange.Year 2025))).ToJSON())

    equal "minute-only time patch" 1 (time.With(TimePatch.Create(TimeChange.Minute 1))).Minute
    equal "month-code patch" "2024-03-29" ((leap.With(DatePatch.Create(DateChange.MonthCode "M03"))).ToJSON())

    let negative =
        duration.Negated().WithMagnitude(DurationPatch.Create(DurationChange.Hours 5u, DurationChange.Minutes 7u))

    equal "negative magnitude keeps sign" -1 negative.Sign
    equal "negative magnitude hours" -5. negative.Hours
    equal "negative magnitude minutes" -7. negative.Minutes

    equal
        "zero magnitude becomes positive"
        1
        (t.Duration.Create().WithMagnitude(DurationPatch.Create(DurationChange.Hours 1u))).Sign

    let unsignedBoundary = t.Duration.CreateTime(nanoseconds = 4294967295u)
    equal "unsigned components stay unsigned" 4294967295. unsignedBoundary.Nanoseconds

    equal
        "large typed nanoseconds stay exact"
        (instant.EpochNanoseconds + 4294967295I)
        (instant.Add(unsignedBoundary)).EpochNanoseconds

    equal
        "fixed duration arithmetic"
        27.
        ((t.Duration.CreateFixed(days = 1u).Add(FixedDurationFields(0u, hours = 3u))).Total(DayTimeUnit.Hour))

    equal "fixed refinement" 24. ((value (Duration.tryAsFixed (t.Duration.Parse("P1D")))).Total(DayTimeUnit.Hour))
    equal "calendar not fixed" true (Duration.tryAsFixed (t.Duration.Parse("P1M")) |> Result.isError)
    equal "days not time-only" true (Duration.tryAsTime (t.Duration.Parse("P1D")) |> Result.isError)
    equal "zero days time-only" true (Duration.tryAsTime (t.Duration.Parse("P0D")) |> Result.isOk)
    let negativeTime = value (Duration.tryAsTime (t.Duration.Parse("-PT1H")))

    equal
        "negative time refinement"
        (instant.EpochNanoseconds - 3600000000000I)
        (instant.Add(negativeTime)).EpochNanoseconds

    equal
        "date style overload"
        "29/02/2024"
        (leap.ToLocaleString(DateStyleOptions(dateStyle = FormatStyle.Short), locales = "en-GB"))

    equal
        "time style overload"
        "12:34"
        (time.ToLocaleString(TimeStyleOptions(timeStyle = FormatStyle.Short), locales = "en-GB"))

    equal
        "datetime style overload"
        true
        ((dt.ToLocaleString(
            DateTimeStyleOptions(dateStyle = FormatStyle.Short, timeStyle = FormatStyle.Short),
            locales = "en-GB"
        ))
            .Length
            >
            0)

    equal
        "instant style overload"
        true
        ((instant.ToLocaleString(
            InstantStyleOptions(dateStyle = FormatStyle.Short, timeZone = "UTC"),
            locales = "en-GB"
        ))
            .Length
            >
            0)

    equal
        "zoned style overload"
        true
        ((before.ToLocaleString(ZonedStyleOptions(timeStyle = FormatStyle.Short), locales = "en-GB")).Length > 0)

    equal
        "time fractional locale digits"
        true
        ((time.ToLocaleString("en-GB", TimeLocaleOptions(fractionalSecondDigits = LocaleSecondDigits.Three))).Length > 0)

    equal
        "year-month locale"
        "2024-02"
        (ym.ToLocaleString(
            "en-GB",
            YearMonthLocaleOptions(calendar = "iso8601", year = NumericStyle.Numeric, month = MonthStyle.TwoDigit)
        ))

    equal
        "month-day locale"
        "02-29"
        (md.ToLocaleString(
            "en-GB",
            MonthDayLocaleOptions(calendar = "iso8601", month = MonthStyle.TwoDigit, day = NumericStyle.TwoDigit)
        ))

    equal
        "duration locale"
        "1 hour"
        (t.Duration
            .CreateTime(hours = 1u)
            .ToLocaleString(
                "en-GB",
                DurationLocaleOptions(style = DurationFormatStyle.Long, fractionalDigits = FractionalDigits.Three)
            ))

    equal "instant value equality" true (Instant.equals instant (t.Instant.From(instant)))
    equal "date value equality" true (PlainDate.equals leap (t.PlainDate.From(leap)))
    equal "time value equality" true (PlainTime.equals time (t.PlainTime.From(time)))
    equal "date-time value equality" true (PlainDateTime.equals dt (t.PlainDateTime.From(dt)))
    equal "year-month value equality" true (PlainYearMonth.equals ym (t.PlainYearMonth.From(ym)))
    equal "month-day value equality" true (PlainMonthDay.equals md (t.PlainMonthDay.From(md)))
    equal "zoned value equality" true (ZonedDateTime.equals before (t.ZonedDateTime.From(before)))
    equal "instant unequal" false (Instant.equals (t.Instant.Create(0I)) (t.Instant.Create(1I)))

    equal
        "sort exact nanoseconds"
        [ 0I; 1I; 2I ]
        ([ t.Instant.Create(2I); t.Instant.Create(0I); t.Instant.Create(1I) ]
         |> List.sortWith Instant.compare
         |> List.map (fun i -> i.EpochNanoseconds))

    equal "date predicate" true ([ t.PlainDate.From(leap) ] |> List.exists (PlainDate.equals leap))
    equal "date ordering" -1 (PlainDate.compare leap (t.PlainDate.Parse("2024-03-01")))
    equal "time ordering" -1 (PlainTime.compare time (t.PlainTime.Parse("13:00")))
    equal "date-time ordering" -1 (PlainDateTime.compare dt (dt.Add("P1D")))
    equal "year-month ordering" -1 (PlainYearMonth.compare ym (ym.Add("P1M")))
    let sameInstant = before.WithTimeZone("UTC")
    equal "zoned instant ordering" 0 (ZonedDateTime.compare before sameInstant)
    equal "zoned identity differs" false (ZonedDateTime.equals before sameInstant)
    let sameDate = leap.WithCalendar("gregory")
    equal "date ordering ignores calendar" 0 (PlainDate.compare leap sameDate)
    equal "date identity includes calendar" false (PlainDate.equals leap sameDate)

    equal
        "instant collection key"
        1
        (Set.ofList [ instant.EpochNanoseconds; (t.Instant.From(instant)).EpochNanoseconds ]
         |> Set.count)

    equal
        "duration comparison with context"
        -1
        (Duration.compare (RelativeTo.Date leap) (t.Duration.Parse("P1M")) (t.Duration.Parse("P30D")))

    equal
        "fixed duration elapsed equality"
        0
        (FixedDuration.compare (t.Duration.CreateTime(hours = 1u)) (t.Duration.CreateTime(minutes = 60u)))

    equal "tryCall success" (Ok 42) (Temporal.tryCall (fun () -> 42))

    match Temporal.tryCall (fun () -> time.Round(TimeRoundOptions(TimeUnit.Minute, roundingIncrement = 7))) with
    | Error(TemporalError.RangeError _) -> ()
    | _ -> failwith "Invalid rounding must return RangeError"

    match
        Temporal.tryCall (fun () ->
            t.PlainDate.From(DateFields(2024, 2, 30), OverflowOptions(overflow = Overflow.Reject)))
    with
    | Error(TemporalError.RangeError _) -> ()
    | _ -> failwith "Rejected overflow must return RangeError"
    // Bypass the typed bigint argument to exercise a real implementation TypeError.
    match Temporal.tryCall (fun () -> emitJsExpr t.Instant "new ($0)(0)": Instant) with
    | Error(TemporalError.TypeError _) -> ()
    | _ -> failwith "Invalid input type must return TypeError"

    let original: exn = emitJsExpr () "new Error('application failure')"

    match Temporal.tryCall (fun () -> raise original) with
    | Error(TemporalError.Other cause as error) ->
        equal "unexpected cause retained" true (emitJsExpr (cause, original) "$0 === $1": bool)
        equal "cause accessor retained" true (emitJsExpr (error.Cause, original) "$0 === $1": bool)
    | _ -> failwith "Application errors must remain Other"
    // A catch boundary must preserve even values that are not JavaScript Error objects.
    match Temporal.tryCall (fun () -> emitJsExpr () "(() => { throw null; })()": unit) with
    | Error(TemporalError.Other cause) -> equal "null thrown value retained" true (emitJsExpr cause "$0 === null": bool)
    | _ -> failwith "Unknown thrown values must remain Other"


    let expectOk label result =
        match result with
        | Ok value ->
            equal (label + " succeeds") true true
            value
        | Error error -> failwithf "%s: %A" label error

    let expectRange label result =
        match result with
        | Error(TemporalError.RangeError _) -> equal label true true
        | _ -> failwithf "%s: expected RangeError" label

    equal
        "Instant TryParse value"
        (t.Instant.Parse("2024-01-01T00:00:00Z").ToJSON())
        ((expectOk "Instant TryParse" (t.Instant.TryParse("2024-01-01T00:00:00Z"))).ToJSON())

    expectRange "Instant TryParse invalid" (t.Instant.TryParse("invalid"))

    equal
        "PlainDate TryParse value"
        (t.PlainDate.Parse("2024-02-29").ToJSON())
        ((expectOk "PlainDate TryParse" (t.PlainDate.TryParse("2024-02-29"))).ToJSON())

    expectRange "PlainDate TryParse invalid" (t.PlainDate.TryParse("invalid"))

    equal
        "PlainTime TryParse value"
        (t.PlainTime.Parse("12:34:56.123456789").ToJSON())
        ((expectOk "PlainTime TryParse" (t.PlainTime.TryParse("12:34:56.123456789"))).ToJSON())

    expectRange "PlainTime TryParse invalid" (t.PlainTime.TryParse("invalid"))

    equal
        "PlainDateTime TryParse value"
        (t.PlainDateTime.Parse("2024-02-29T12:34:56.123456789").ToJSON())
        ((expectOk "PlainDateTime TryParse" (t.PlainDateTime.TryParse("2024-02-29T12:34:56.123456789"))).ToJSON())

    expectRange "PlainDateTime TryParse invalid" (t.PlainDateTime.TryParse("invalid"))

    equal
        "PlainYearMonth TryParse value"
        (t.PlainYearMonth.Parse("2024-02").ToJSON())
        ((expectOk "PlainYearMonth TryParse" (t.PlainYearMonth.TryParse("2024-02"))).ToJSON())

    expectRange "PlainYearMonth TryParse invalid" (t.PlainYearMonth.TryParse("invalid"))

    equal
        "PlainMonthDay TryParse value"
        (t.PlainMonthDay.Parse("02-29").ToJSON())
        ((expectOk "PlainMonthDay TryParse" (t.PlainMonthDay.TryParse("02-29"))).ToJSON())

    expectRange "PlainMonthDay TryParse invalid" (t.PlainMonthDay.TryParse("invalid"))

    equal
        "ZonedDateTime TryParse value"
        (t.ZonedDateTime.Parse("2024-01-01T00:00+00:00[UTC]").ToJSON())
        ((expectOk "ZonedDateTime TryParse" (t.ZonedDateTime.TryParse("2024-01-01T00:00+00:00[UTC]"))).ToJSON())

    expectRange "ZonedDateTime TryParse invalid" (t.ZonedDateTime.TryParse("invalid"))

    equal
        "Duration TryParse value"
        (t.Duration.Parse("-PT1H").ToJSON())
        ((expectOk "Duration TryParse" (t.Duration.TryParse("-PT1H"))).ToJSON())

    expectRange "Duration TryParse invalid" (t.Duration.TryParse("invalid"))

    equal
        "TryFrom retains constrain"
        "2024-02-29"
        ((expectOk "TryFrom constrained" (t.PlainDate.TryFrom(DateFields(2024, 2, 30)))).ToJSON())

    expectRange
        "TryFrom forwards reject"
        (t.PlainDate.TryFrom(DateFields(2024, 2, 30), OverflowOptions(overflow = Overflow.Reject)))

    expectRange "TryCreate rejects invalid date" (t.PlainDate.TryCreate(2024, 2, 30))

    equal
        "TryCreate calendar forwarding"
        "gregory"
        (expectOk "date constructor" (t.PlainDate.TryCreate(2024, 2, 29, calendar = "gregory"))).CalendarId

    equal
        "TryCreate time optional fields"
        "00:02:03.004005006"
        ((expectOk
            "time constructor"
            (t.PlainTime.TryCreate(minute = 2, second = 3, millisecond = 4, microsecond = 5, nanosecond = 6)))
            .ToJSON())

    equal "TryCreate time defaults" "00:00:00" ((expectOk "time defaults" (t.PlainTime.TryCreate())).ToJSON())
    expectRange "TryCreate invalid time" (t.PlainTime.TryCreate(hour = 24))

    expectRange
        "TryFrom invalid time"
        (t.PlainTime.TryFrom(TimeFields(25), OverflowOptions(overflow = Overflow.Reject)))

    let createdDateTime =
        expectOk
            "date-time constructor"
            (t.PlainDateTime.TryCreate(
                2024,
                2,
                29,
                hour = 1,
                minute = 2,
                second = 3,
                millisecond = 4,
                microsecond = 5,
                nanosecond = 6,
                calendar = "gregory"
            ))

    equal "TryCreate date-time all arguments" "2024-02-29T01:02:03.004005006[u-ca=gregory]" (createdDateTime.ToJSON())

    expectRange
        "TryFrom date-time reject"
        (t.PlainDateTime.TryFrom(DateTimeFields(2024, 2, 30), OverflowOptions(overflow = Overflow.Reject)))

    equal
        "TryFrom date-time defaults"
        "2024-02-29T00:00:00"
        ((expectOk "date-time fields" (t.PlainDateTime.TryFrom(DateTimeFields(2024, 2, 30)))).ToJSON())

    equal
        "TryCreate year-month reference day"
        "2024-02-15[u-ca=gregory]"
        ((expectOk
            "year-month constructor"
            (t.PlainYearMonth.TryCreate(2024, 2, calendar = "gregory", referenceISODay = 15)))
            .ToJSON())

    expectRange
        "TryFrom year-month reject"
        (t.PlainYearMonth.TryFrom(YearMonthFields(2024, 13), OverflowOptions(overflow = Overflow.Reject)))

    equal
        "TryFrom year-month constrain"
        12
        (expectOk "year-month fields" (t.PlainYearMonth.TryFrom(YearMonthFields(2024, 13)))).Month

    equal
        "TryCreate month-day reference year"
        "2024-02-29[u-ca=gregory]"
        ((expectOk
            "month-day constructor"
            (t.PlainMonthDay.TryCreate(2, 29, calendar = "gregory", referenceISOYear = 2024)))
            .ToJSON())

    expectRange
        "TryFrom month-day reject"
        (t.PlainMonthDay.TryFrom(MonthDayFields("M02", 30), OverflowOptions(overflow = Overflow.Reject)))

    equal
        "TryFrom month-day constrain"
        29
        (expectOk "month-day fields" (t.PlainMonthDay.TryFrom(MonthDayFields("M02", 30)))).Day

    equal "TryCreate instant exact" 1I (expectOk "instant constructor" (t.Instant.TryCreate(1I))).EpochNanoseconds
    expectRange "TryCreate instant bounds" (t.Instant.TryCreate(8640000000000000000001I))

    equal
        "TryFromEpochNanoseconds exact"
        -1I
        (expectOk "epoch nanoseconds" (t.Instant.TryFromEpochNanoseconds(-1I))).EpochNanoseconds

    expectRange "TryFromEpochNanoseconds bounds" (t.Instant.TryFromEpochNanoseconds(8640000000000000000001I))

    equal
        "TryFromEpochMilliseconds"
        1000000I
        (expectOk "epoch milliseconds" (t.Instant.TryFromEpochMilliseconds(1.))).EpochNanoseconds

    expectRange "TryFromEpochMilliseconds fractional" (t.Instant.TryFromEpochMilliseconds(1.5))

    equal
        "TryCreate zoned calendar"
        "gregory"
        (expectOk "zoned constructor" (t.ZonedDateTime.TryCreate(0I, "UTC", calendar = "gregory"))).CalendarId

    expectRange "TryCreate invalid zone" (t.ZonedDateTime.TryCreate(0I, "Invalid/Zone"))

    let repeated =
        ZonedDateTimeFields(2024, 4, 7, "Australia/Melbourne", hour = 2, minute = 30)

    expectRange
        "TryFrom zoned ambiguity"
        (t.ZonedDateTime.TryFrom(repeated, ZonedOptions(disambiguation = Disambiguation.Reject)))

    let earlyZoned = expectOk "zoned fields default" (t.ZonedDateTime.TryFrom(repeated))

    let lateZoned =
        expectOk
            "zoned fields later"
            (t.ZonedDateTime.TryFrom(repeated, ZonedOptions(disambiguation = Disambiguation.Later)))

    equal "TryFrom zoned disambiguation" 3600000000000I (lateZoned.EpochNanoseconds - earlyZoned.EpochNanoseconds)

    expectRange
        "TryParse zoned offset forwarding"
        (t.ZonedDateTime.TryParse("2024-01-01T00:00+01:00[UTC]", ZonedOptions(offset = OffsetPolicy.Reject)))

    expectOk
        "TryParse zoned offset ignore"
        (t.ZonedDateTime.TryParse("2024-01-01T00:00+01:00[UTC]", ZonedOptions(offset = OffsetPolicy.Ignore)))
    |> ignore

    let general =
        expectOk
            "duration constructor"
            (t.Duration.TryCreate(
                years = 1u,
                months = 2u,
                weeks = 3u,
                days = 4u,
                hours = 5u,
                minutes = 6u,
                seconds = 7u,
                milliseconds = 8u,
                microseconds = 9u,
                nanoseconds = 10u
            ))

    equal "TryCreate duration all arguments" "P1Y2M3W4DT5H6M7.00800901S" (general.ToJSON())

    let timeOnly: TimeDuration =
        expectOk "time duration constructor" (t.Duration.TryCreateTime(minutes = 2u, nanoseconds = 3u))

    equal
        "TryCreateTime retains instant capability"
        120000000003I
        ((t.Instant.Create(0I)).Add(timeOnly)).EpochNanoseconds

    let fixedOnly: FixedDuration =
        expectOk "fixed duration constructor" (t.Duration.TryCreateFixed(days = 1u, hours = 2u))

    equal "TryCreateFixed retains total capability" 26. (fixedOnly.Total(DayTimeUnit.Hour))

    equal
        "TryFrom duration fields"
        "PT2H"
        ((expectOk "duration fields" (t.Duration.TryFrom(DurationFields(0u, hours = 2u)))).ToJSON())

    let localRepeated = t.PlainDateTime.Parse("2024-04-07T02:30")

    expectRange
        "TryToZonedDateTime repeated reject"
        (localRepeated.TryToZonedDateTime(
            "Australia/Melbourne",
            DisambiguationOptions(disambiguation = Disambiguation.Reject)
        ))

    expectRange
        "TryToZonedDateTime skipped reject"
        (t.PlainDateTime
            .Parse("2024-10-06T02:30")
            .TryToZonedDateTime("Australia/Melbourne", DisambiguationOptions(disambiguation = Disambiguation.Reject)))

    equal
        "TryToZonedDateTime default compatible"
        earlyZoned.EpochNanoseconds
        (expectOk "local conversion" (localRepeated.TryToZonedDateTime("Australia/Melbourne"))).EpochNanoseconds

    equal
        "TryToZonedDateTime later forwarding"
        lateZoned.EpochNanoseconds
        (expectOk
            "local conversion later"
            (localRepeated.TryToZonedDateTime(
                "Australia/Melbourne",
                DisambiguationOptions(disambiguation = Disambiguation.Later)
            )))
            .EpochNanoseconds

    equal "TryToZonedDateTime date defaults" 0 (expectOk "date conversion" (leap.TryToZonedDateTime("UTC"))).Hour

    equal
        "TryToZonedDateTime date options"
        12
        (expectOk
            "date options conversion"
            (leap.TryToZonedDateTime(DateToZonedOptions("UTC", plainTime = t.PlainTime.Create(12)))))
            .Hour

    expectRange "TryToZonedDateTime date invalid zone" (leap.TryToZonedDateTime("Invalid/Zone"))

    equal
        "TryToZonedDateTimeISO exact"
        instant.EpochNanoseconds
        (expectOk "instant zone conversion" (instant.TryToZonedDateTimeISO("UTC"))).EpochNanoseconds

    expectRange "TryToZonedDateTimeISO invalid zone" (instant.TryToZonedDateTimeISO("Invalid/Zone"))

    printfn "Passed %s" name

if TemporalRuntime.IsNativeAvailable then
    run "native Temporal" TemporalRuntime.Native

run "temporal-polyfill/full/implementation" (TemporalRuntime(polyfill))

// Restore the application global after testing late global lookup.
let saved: obj = emitJsExpr () "globalThis.Temporal"

try
    emitJsStatement () "delete globalThis.Temporal"
    equal "native unavailable" false TemporalRuntime.IsNativeAvailable
    let mutable unavailable = false

    try
        TemporalRuntime.Native |> ignore
    with _ ->
        unavailable <- true

    equal "missing implementation message" true unavailable
    emitJsStatement polyfill "globalThis.Temporal = $0"
    equal "global available" true TemporalRuntime.IsNativeAvailable
    equal "late global lookup" 1I (TemporalRuntime.Native.Instant.FromEpochNanoseconds(1I)).EpochNanoseconds
finally
    emitJsStatement saved "if ($0 === undefined) delete globalThis.Temporal; else globalThis.Temporal = $0"

printfn "Passed %d assertions" assertions
