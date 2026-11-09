using ClubBooking.Bookings.Domain;
using ClubBooking.Bookings.Models;

namespace ClubBooking.Bookings.Tests;

public class BookingRulesTests
{
    private static readonly TimeZoneInfo Moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
    private static readonly DateTime Now = new(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    private static DateTime MoscowTime(int day, int hour, int minute = 0) =>
        TimeZoneInfo.ConvertTimeToUtc(new DateTime(2026, 11, day, hour, minute, 0), Moscow);

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void CheckNew_AllowsFromOneToFiveHours(int hours)
    {
        Assert.Null(BookingRules.CheckNew(Now.AddDays(1), hours, Now, Moscow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CheckNew_RejectsDurationOutsideOneToFiveHours(int hours)
    {
        Assert.NotNull(BookingRules.CheckNew(Now.AddDays(1), hours, Now, Moscow));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(30, true)]
    [InlineData(15, false)]
    [InlineData(45, false)]
    public void CheckNew_AllowsStartOnlyAtZeroOrThirtyMinutes(int minute, bool allowed)
    {
        var error = BookingRules.CheckNew(MoscowTime(6, 19, minute), 2, Now, Moscow);

        Assert.Equal(allowed, error == null);
    }

    [Fact]
    public void CheckNew_RejectsStartInThePast()
    {
        Assert.NotNull(BookingRules.CheckNew(Now.AddMinutes(-30), 1, Now, Moscow));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    public void CheckLimit_AllowsAtMostTwoFutureBookings(int existing, bool allowed)
    {
        Assert.Equal(allowed, BookingRules.CheckLimit(existing) == null);
    }

    [Theory]
    [InlineData(120, true)]
    [InlineData(60, true)]
    [InlineData(59, false)]
    [InlineData(-10, false)]
    public void CanCancel_UntilOneHourBeforeStart(int minutesBeforeStart, bool allowed)
    {
        var start = Now.AddMinutes(minutesBeforeStart);

        Assert.Equal(allowed, BookingRules.CanCancel(start, Now));
    }

    [Theory]
    [InlineData(-16, false)]
    [InlineData(-15, true)]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void CanCheckIn_FromFifteenMinutesBeforeToFifteenAfterStart(int minutesFromStart, bool allowed)
    {
        var start = Now.AddMinutes(-minutesFromStart);

        Assert.Equal(allowed, BookingRules.CanCheckIn(start, Now));
    }

    [Theory]
    [InlineData(BookingStatus.Booked, 16, true)]
    [InlineData(BookingStatus.Booked, 15, false)]
    [InlineData(BookingStatus.CheckedIn, 60, false)]
    [InlineData(BookingStatus.Cancelled, 60, false)]
    public void IsNoShow_WhenBookedAndNotCheckedInFifteenMinutesAfterStart(BookingStatus status, int minutesAfterStart, bool expected)
    {
        Assert.Equal(expected, BookingRules.IsNoShow(status, Now.AddMinutes(-minutesAfterStart), Now));
    }

    [Fact]
    public void Price_DayTime_UsesDayRate()
    {
        Assert.Equal(240, BookingRules.Price(MoscowTime(6, 18), 2, 120, 80, Moscow));
    }

    [Fact]
    public void Price_NightTime_UsesNightRate()
    {
        Assert.Equal(240, BookingRules.Price(MoscowTime(6, 2), 3, 120, 80, Moscow));
    }

    [Fact]
    public void Price_AcrossMidnight_SumsHalfHoursByTheirRate()
    {
        Assert.Equal(80, BookingRules.Price(MoscowTime(6, 23, 30), 1, 100, 60, Moscow));
    }

    [Fact]
    public void Price_IsRoundedToKopecks()
    {
        Assert.Equal(90.51m, BookingRules.Price(MoscowTime(6, 23, 30), 1, 101.01m, 80, Moscow));
    }

    [Fact]
    public void Price_AcrossMorning_SwitchesToDayRateAtEight()
    {
        Assert.Equal(160, BookingRules.Price(MoscowTime(6, 7), 2, 100, 60, Moscow));
    }
}
