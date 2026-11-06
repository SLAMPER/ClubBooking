using System.ComponentModel.DataAnnotations;

namespace ClubBooking.Seats.Models;

public record ZoneRequest(
    [Required, StringLength(50)] string Name,
    [Range(0, 100000)] decimal DayRate,
    [Range(0, 100000)] decimal NightRate);

public record ZoneDto(int Id, string Name, decimal DayRate, decimal NightRate);

public record SeatRequest(
    int ZoneId,
    [Required, StringLength(10)] string Number,
    [Range(1, 20)] int Row,
    [Range(1, 20)] int Col);

public record SeatDto(
    int Id,
    string Number,
    int Row,
    int Col,
    bool IsActive,
    int ZoneId,
    string ZoneName,
    decimal DayRate,
    decimal NightRate);
