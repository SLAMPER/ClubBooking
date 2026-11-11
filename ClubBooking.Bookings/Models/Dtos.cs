using System.ComponentModel.DataAnnotations;

namespace ClubBooking.Bookings.Models;

public record CreateBookingRequest(int SeatId, DateTimeOffset Start, [Range(1, 5, ErrorMessage = "Бронь может длиться от 1 до 5 часов")] int Hours);

public record BookingDto(
    Guid Id,
    int SeatId,
    string SeatNumber,
    string ZoneName,
    DateTime StartUtc,
    DateTime EndUtc,
    decimal Price,
    string Status,
    string UserEmail,
    bool CanCancel,
    bool CanCheckIn);

public record PriceDto(decimal Price);

public record SeatInfo(int Id, string Number, bool IsActive, string ZoneName, decimal DayRate, decimal NightRate);
