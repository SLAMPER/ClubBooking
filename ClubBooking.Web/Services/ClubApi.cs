using System.Text.Json;

namespace ClubBooking.Web.Services;

public class ClubApi(IHttpClientFactory factory)
{
    private HttpClient Identity => factory.CreateClient("identity");
    private HttpClient Seats => factory.CreateClient("seats");
    private HttpClient Bookings => factory.CreateClient("bookings");

    public Task<ApiResult<TokenResponse>> LoginAsync(string email, string password) =>
        ReadAsync<TokenResponse>(Identity.PostAsJsonAsync("/api/auth/login", new { email, password }));

    public Task<ApiResult<TokenResponse>> RegisterAsync(string email, string password) =>
        ReadAsync<TokenResponse>(Identity.PostAsJsonAsync("/api/auth/register", new { email, password }));

    public async Task<List<ZoneDto>> GetZonesAsync() =>
        await Seats.GetFromJsonAsync<List<ZoneDto>>("/api/zones") ?? [];

    public async Task<List<SeatDto>> GetSeatsAsync() =>
        await Seats.GetFromJsonAsync<List<SeatDto>>("/api/seats") ?? [];

    public Task<ApiResult> SaveZoneAsync(int? id, string name, decimal dayRate, decimal nightRate) =>
        SendAsync(id == null
            ? Seats.PostAsJsonAsync("/api/zones", new { name, dayRate, nightRate })
            : Seats.PutAsJsonAsync($"/api/zones/{id}", new { name, dayRate, nightRate }));

    public Task<ApiResult> DeleteZoneAsync(int id) => SendAsync(Seats.DeleteAsync($"/api/zones/{id}"));

    public Task<ApiResult> SaveSeatAsync(int? id, int zoneId, string number, int row, int col) =>
        SendAsync(id == null
            ? Seats.PostAsJsonAsync("/api/seats", new { zoneId, number, row, col })
            : Seats.PutAsJsonAsync($"/api/seats/{id}", new { zoneId, number, row, col }));

    public Task<ApiResult> SetSeatActiveAsync(int id, bool isActive) =>
        SendAsync(Seats.PostAsync($"/api/seats/{id}/{(isActive ? "open" : "close")}", null));

    public async Task<HashSet<int>> GetBusyAsync(DateTime fromUtc, DateTime toUtc) =>
        (await Bookings.GetFromJsonAsync<List<int>>($"/api/bookings/busy?from={Format(fromUtc)}&to={Format(toUtc)}") ?? []).ToHashSet();

    public Task<ApiResult<PriceDto>> GetPriceAsync(int seatId, DateTime startUtc, int hours) =>
        ReadAsync<PriceDto>(Bookings.GetAsync($"/api/bookings/price?seatId={seatId}&start={Format(startUtc)}&hours={hours}"));

    public Task<ApiResult<BookingDto>> CreateBookingAsync(int seatId, DateTime startUtc, int hours) =>
        ReadAsync<BookingDto>(Bookings.PostAsJsonAsync("/api/bookings", new { seatId, start = startUtc, hours }));

    public async Task<List<BookingDto>> GetMyBookingsAsync() =>
        await Bookings.GetFromJsonAsync<List<BookingDto>>("/api/bookings/my") ?? [];

    public Task<ApiResult> CancelBookingAsync(Guid id) => SendAsync(Bookings.PostAsync($"/api/bookings/{id}/cancel", null));

    public async Task<List<BookingDto>> GetTodayAsync() =>
        await Bookings.GetFromJsonAsync<List<BookingDto>>("/api/bookings/today") ?? [];

    public Task<ApiResult> CheckInAsync(Guid id) => SendAsync(Bookings.PostAsync($"/api/bookings/{id}/checkin", null));

    private static string Format(DateTime utc) => Uri.EscapeDataString(DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O"));

    private static async Task<ApiResult> SendAsync(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        return response.IsSuccessStatusCode ? new ApiResult(true, null) : new ApiResult(false, await ErrorAsync(response));
    }

    private static async Task<ApiResult<T>> ReadAsync<T>(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<T>(default, await ErrorAsync(response));
        }

        return new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>(), null);
    }

    private static async Task<string> ErrorAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            using var json = JsonDocument.Parse(text);
            if (json.RootElement.TryGetProperty("errors", out var errors))
            {
                return string.Join(" ", errors.EnumerateObject().SelectMany(e => e.Value.EnumerateArray()).Select(m => m.GetString()));
            }
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return "Недостаточно прав";
        }

        return string.IsNullOrWhiteSpace(text) ? $"Ошибка сервиса ({(int)response.StatusCode})" : text;
    }
}
