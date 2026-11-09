using ClubBooking.Bookings.Data;
using ClubBooking.Bookings.Domain;
using ClubBooking.Bookings.Models;
using ClubBooking.Bookings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClubBooking.Bookings.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(BookingsDbContext db, ISeatsClient seats, TimeZoneInfo club) : ControllerBase
{
    [HttpGet("busy")]
    public async Task<List<int>> Busy(DateTimeOffset from, DateTimeOffset to)
    {
        var now = DateTime.UtcNow;
        var expired = now - BookingRules.CheckInWindow;

        return await db.Bookings
            .Where(b => b.StartUtc < to.UtcDateTime && b.EndUtc > from.UtcDateTime)
            .Where(b => b.Status == BookingStatus.CheckedIn || (b.Status == BookingStatus.Booked && b.StartUtc >= expired))
            .Select(b => b.SeatId)
            .Distinct()
            .ToListAsync();
    }

    [HttpGet("price")]
    public async Task<ActionResult<PriceDto>> Price(int seatId, DateTimeOffset start, int hours)
    {
        var startUtc = start.UtcDateTime;
        var error = BookingRules.CheckNew(startUtc, hours, DateTime.UtcNow, club);
        if (error != null)
        {
            return BadRequest(error);
        }

        var seat = await seats.GetSeatAsync(seatId);
        if (seat == null)
        {
            return NotFound("Такого места нет");
        }

        return new PriceDto(BookingRules.Price(startUtc, hours, seat.DayRate, seat.NightRate, club));
    }

    [HttpPost]
    [Authorize(Roles = "client")]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request)
    {
        var now = DateTime.UtcNow;
        var startUtc = request.Start.UtcDateTime;
        var error = BookingRules.CheckNew(startUtc, request.Hours, now, club);
        if (error != null)
        {
            return BadRequest(error);
        }

        var seat = await seats.GetSeatAsync(request.SeatId);
        if (seat == null)
        {
            return BadRequest("Такого места нет");
        }

        if (!seat.IsActive)
        {
            return BadRequest("Место закрыто на ремонт");
        }

        var userId = UserId();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = User.FindFirst("email")?.Value ?? "",
            SeatId = seat.Id,
            SeatNumber = seat.Number,
            ZoneName = seat.ZoneName,
            StartUtc = startUtc,
            EndUtc = startUtc.AddHours(request.Hours),
            Price = BookingRules.Price(startUtc, request.Hours, seat.DayRate, seat.NightRate, club),
            Status = BookingStatus.Booked,
            CreatedAt = now
        };

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtext({userId.ToString()}))");

        var expired = now - BookingRules.CheckInWindow;
        await db.Bookings
            .Where(b => b.SeatId == seat.Id && b.Status == BookingStatus.Booked && b.StartUtc < expired)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.NoShow));

        var future = await db.Bookings.CountAsync(b => b.UserId == userId && b.Status == BookingStatus.Booked && b.StartUtc > now);
        var limitError = BookingRules.CheckLimit(future);
        if (limitError != null)
        {
            return BadRequest(limitError);
        }

        db.Bookings.Add(booking);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
        {
            return Conflict("Это место уже занято на выбранное время");
        }

        await transaction.CommitAsync();
        return Created($"/api/bookings/{booking.Id}", ToDto(booking, now));
    }

    [HttpGet("my")]
    [Authorize(Roles = "client")]
    public async Task<List<BookingDto>> My()
    {
        var userId = UserId();
        var bookings = await db.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartUtc)
            .ToListAsync();

        var now = DateTime.UtcNow;
        return bookings.Select(b => ToDto(b, now)).ToList();
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "client")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId());
        if (booking == null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        if (BookingRules.EffectiveStatus(booking.Status, booking.StartUtc, now) != BookingStatus.Booked)
        {
            return BadRequest("Эту бронь уже нельзя отменить");
        }

        if (!BookingRules.CanCancel(booking.StartUtc, now))
        {
            return BadRequest("Отменить бронь можно не позже чем за 1 час до начала");
        }

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("today")]
    [Authorize(Roles = "admin")]
    public async Task<List<BookingDto>> Today()
    {
        var now = DateTime.UtcNow;
        var today = TimeZoneInfo.ConvertTimeFromUtc(now, club).Date;
        var from = TimeZoneInfo.ConvertTimeToUtc(today, club);
        var to = TimeZoneInfo.ConvertTimeToUtc(today.AddDays(1), club);

        var bookings = await db.Bookings
            .Where(b => b.StartUtc >= from && b.StartUtc < to)
            .OrderBy(b => b.StartUtc).ThenBy(b => b.SeatNumber)
            .ToListAsync();

        return bookings.Select(b => ToDto(b, now)).ToList();
    }

    [HttpPost("{id}/checkin")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        if (booking.Status != BookingStatus.Booked || !BookingRules.CanCheckIn(booking.StartUtc, now))
        {
            return BadRequest("Отметить приход можно с 15 минут до начала до 15 минут после");
        }

        booking.Status = BookingStatus.CheckedIn;
        booking.CheckedInAt = now;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Guid UserId() => Guid.Parse(User.FindFirst("sub")!.Value);

    private static BookingDto ToDto(Booking booking, DateTime now)
    {
        var status = BookingRules.EffectiveStatus(booking.Status, booking.StartUtc, now);
        return new BookingDto(
            booking.Id,
            booking.SeatId,
            booking.SeatNumber,
            booking.ZoneName,
            booking.StartUtc,
            booking.EndUtc,
            booking.Price,
            status.ToString(),
            booking.UserEmail,
            status == BookingStatus.Booked && BookingRules.CanCancel(booking.StartUtc, now),
            status == BookingStatus.Booked && BookingRules.CanCheckIn(booking.StartUtc, now));
    }
}
