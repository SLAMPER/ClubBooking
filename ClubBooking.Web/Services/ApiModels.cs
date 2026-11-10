namespace ClubBooking.Web.Services;

public record TokenResponse(string Token, string Email, string Role, DateTime ExpiresAt);

public record ZoneDto(int Id, string Name, decimal DayRate, decimal NightRate);

public record SeatDto(int Id, string Number, int Row, int Col, bool IsActive, int ZoneId, string ZoneName, decimal DayRate, decimal NightRate);

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

public record ApiResult(bool Ok, string? Error);

public record ApiResult<T>(T? Value, string? Error);
