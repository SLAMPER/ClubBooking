using ClubBooking.Bookings.Models;

namespace ClubBooking.Bookings.Domain;

public static class BookingRules
{
    public const int MinHours = 1;
    public const int MaxHours = 5;
    public const int MaxFutureBookings = 2;
    public const int NightEndHour = 8;
    public static readonly TimeSpan CancelBefore = TimeSpan.FromHours(1);
    public static readonly TimeSpan CheckInWindow = TimeSpan.FromMinutes(15);

    public static string? CheckNew(DateTime startUtc, int hours, DateTime nowUtc, TimeZoneInfo club)
    {
        if (hours < MinHours || hours > MaxHours)
        {
            return "Бронь может длиться от 1 до 5 часов";
        }

        var local = TimeZoneInfo.ConvertTimeFromUtc(startUtc, club);
        if (local.Minute % 30 != 0 || local.Second != 0 || local.Millisecond != 0)
        {
            return "Бронь начинается в 00 или 30 минут";
        }

        if (startUtc <= nowUtc)
        {
            return "Нельзя забронировать время, которое уже прошло";
        }

        return null;
    }

    public static string? CheckLimit(int futureBookings)
    {
        return futureBookings >= MaxFutureBookings ? "Можно иметь не больше 2 будущих броней" : null;
    }

    public static bool CanCancel(DateTime startUtc, DateTime nowUtc)
    {
        return nowUtc <= startUtc - CancelBefore;
    }

    public static bool CanCheckIn(DateTime startUtc, DateTime nowUtc)
    {
        return nowUtc >= startUtc - CheckInWindow && nowUtc <= startUtc + CheckInWindow;
    }

    public static bool IsNoShow(BookingStatus status, DateTime startUtc, DateTime nowUtc)
    {
        return status == BookingStatus.Booked && nowUtc > startUtc + CheckInWindow;
    }

    public static BookingStatus EffectiveStatus(BookingStatus status, DateTime startUtc, DateTime nowUtc)
    {
        return IsNoShow(status, startUtc, nowUtc) ? BookingStatus.NoShow : status;
    }

    public static decimal Price(DateTime startUtc, int hours, decimal dayRate, decimal nightRate, TimeZoneInfo club)
    {
        decimal total = 0;
        for (var i = 0; i < hours * 2; i++)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(startUtc.AddMinutes(30 * i), club);
            total += (local.Hour < NightEndHour ? nightRate : dayRate) / 2;
        }

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
