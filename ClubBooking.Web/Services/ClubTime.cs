namespace ClubBooking.Web.Services;

public class ClubTime(IConfiguration configuration)
{
    private readonly TimeZoneInfo _zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Club:TimeZone"]!);

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zone);

    public DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _zone);

    public DateTime ToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), _zone);

    public static string StatusClass(string status) => "cb-status is-" + status.ToLowerInvariant();

    public static string StatusText(string status) => status switch
    {
        "Booked" => "Забронировано",
        "CheckedIn" => "Клиент пришёл",
        "Cancelled" => "Отменено",
        "NoShow" => "Не пришёл",
        _ => status
    };
}
