using System.Net;
using System.Net.Http.Json;
using ClubBooking.Bookings.Data;
using ClubBooking.Bookings.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClubBooking.Bookings.Tests;

public class BookingsApiTests(BookingsApiFactory factory) : IClassFixture<BookingsApiFactory>
{
    private static int _nextSeat = 100;

    private static DateTimeOffset Tomorrow(int hour) =>
        new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1).AddHours(hour), TimeSpan.Zero);

    private int NewSeat(bool isActive = true)
    {
        var id = Interlocked.Increment(ref _nextSeat);
        factory.Seats.Add(id, isActive);
        return id;
    }

    [Fact]
    public async Task TwoClients_BookSameSeatAtSameTime_OnlyOneSucceeds()
    {
        var seat = NewSeat();
        var request = new CreateBookingRequest(seat, Tomorrow(12), 2);

        var responses = await Task.WhenAll(
            factory.CreateClient(Guid.NewGuid()).PostAsJsonAsync("/api/bookings", request),
            factory.CreateClient(Guid.NewGuid()).PostAsJsonAsync("/api/bookings", request));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Booking_OverlappingTime_ReturnsConflict_ButAdjacentTimeIsAllowed()
    {
        var seat = NewSeat();
        await factory.CreateClient(Guid.NewGuid()).PostAsJsonAsync("/api/bookings", new CreateBookingRequest(seat, Tomorrow(12), 2));

        var overlapping = await factory.CreateClient(Guid.NewGuid()).PostAsJsonAsync("/api/bookings", new CreateBookingRequest(seat, Tomorrow(13), 2));
        var adjacent = await factory.CreateClient(Guid.NewGuid()).PostAsJsonAsync("/api/bookings", new CreateBookingRequest(seat, Tomorrow(14), 2));

        Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
        Assert.Equal(HttpStatusCode.Created, adjacent.StatusCode);
    }

    [Fact]
    public async Task ParallelBookings_OfOneClient_DoNotExceedTheLimit()
    {
        var client = factory.CreateClient(Guid.NewGuid());
        await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(NewSeat(), Tomorrow(8), 1));

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(NewSeat(), Tomorrow(10), 1))));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Equal(4, responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Booking_ClosedSeat_ReturnsBadRequest()
    {
        var response = await factory.CreateClient(Guid.NewGuid())
            .PostAsJsonAsync("/api/bookings", new CreateBookingRequest(NewSeat(isActive: false), Tomorrow(12), 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Booking_ReturnsPriceByZoneRate()
    {
        var response = await factory.CreateClient(Guid.NewGuid())
            .PostAsJsonAsync("/api/bookings", new CreateBookingRequest(NewSeat(), Tomorrow(12), 3));
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();

        Assert.Equal(360, booking!.Price);
    }

    [Fact]
    public async Task Booking_SeatOfExpiredHold_IsAllowed_AndExpiredHoldBecomesNoShow()
    {
        var seat = NewSeat();
        var expiredId = await AddBookingAsync(seat, DateTime.UtcNow.AddMinutes(-20), 3);

        var response = await factory.CreateClient(Guid.NewGuid())
            .PostAsJsonAsync("/api/bookings", new CreateBookingRequest(seat, NextHalfHour(), 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        Assert.Equal(BookingStatus.NoShow, (await db.Bookings.FindAsync(expiredId))!.Status);
    }

    [Fact]
    public async Task Busy_DoesNotIncludeSeatOfExpiredHold()
    {
        var expiredSeat = NewSeat();
        var busySeat = NewSeat();
        await AddBookingAsync(expiredSeat, DateTime.UtcNow.AddMinutes(-20), 3);
        await AddBookingAsync(busySeat, DateTime.UtcNow.AddMinutes(-10), 3);
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddHours(1).ToString("O"));

        var busy = await factory.CreateClient().GetFromJsonAsync<List<int>>($"/api/bookings/busy?from={from}&to={to}");

        Assert.Contains(busySeat, busy!);
        Assert.DoesNotContain(expiredSeat, busy!);
    }

    [Fact]
    public async Task Cancel_BookingOfAnotherClient_ReturnsNotFound()
    {
        var created = await factory.CreateClient(Guid.NewGuid())
            .PostAsJsonAsync("/api/bookings", new CreateBookingRequest(NewSeat(), Tomorrow(12), 1));
        var booking = await created.Content.ReadFromJsonAsync<BookingDto>();

        var response = await factory.CreateClient(Guid.NewGuid()).PostAsync($"/api/bookings/{booking!.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Today_AsClient_ReturnsForbidden()
    {
        var response = await factory.CreateClient(Guid.NewGuid()).GetAsync("/api/bookings/today");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static DateTimeOffset NextHalfHour()
    {
        var now = DateTime.UtcNow;
        var next = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute < 30 ? 30 : 0, 0, DateTimeKind.Utc);
        return new DateTimeOffset(now.Minute < 30 ? next : next.AddHours(1), TimeSpan.Zero);
    }

    private async Task<Guid> AddBookingAsync(int seat, DateTime startUtc, int hours)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SeatId = seat,
            StartUtc = startUtc,
            EndUtc = startUtc.AddHours(hours),
            Status = BookingStatus.Booked,
            CreatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking.Id;
    }
}
