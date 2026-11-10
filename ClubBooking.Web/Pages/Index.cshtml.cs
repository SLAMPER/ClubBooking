using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages;

public record ZoneView(ZoneDto Zone, List<SeatDto> Seats, int Rows, int Cols);

public class IndexModel(ClubApi api, ClubTime time) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public DateTime? Date { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Time { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Hours { get; set; } = 2;

    [BindProperty(SupportsGet = true)]
    public int? Seat { get; set; }

    public List<ZoneView> Zones { get; private set; } = [];
    public HashSet<int> Busy { get; private set; } = [];
    public HashSet<int> Mine { get; private set; } = [];
    public SeatDto? Selected { get; private set; }
    public decimal? Price { get; private set; }
    public string? PriceError { get; private set; }
    public DateTime StartLocal { get; private set; }
    public DateTime EndLocal => StartLocal.AddHours(Hours);
    public DateTime Today => time.Now.Date;

    public static IEnumerable<string> TimeOptions =>
        Enumerable.Range(0, 48).Select(i => TimeSpan.FromMinutes(30 * i).ToString(@"hh\:mm"));

    public async Task OnGetAsync()
    {
        Normalize();
        var startUtc = time.ToUtc(StartLocal);
        var endUtc = startUtc.AddHours(Hours);

        var zones = await api.GetZonesAsync();
        var seats = await api.GetSeatsAsync();
        Busy = await api.GetBusyAsync(startUtc, endUtc);

        if (User.IsInRole("client"))
        {
            Mine = (await api.GetMyBookingsAsync())
                .Where(b => b.Status is "Booked" or "CheckedIn" && b.StartUtc < endUtc && b.EndUtc > startUtc)
                .Select(b => b.SeatId)
                .ToHashSet();
        }

        foreach (var zone in zones)
        {
            var zoneSeats = seats.Where(s => s.ZoneId == zone.Id).ToList();
            var rows = zoneSeats.Select(s => s.Row).DefaultIfEmpty(1).Max();
            var cols = zoneSeats.Select(s => s.Col).DefaultIfEmpty(1).Max();
            Zones.Add(new ZoneView(zone, zoneSeats, rows, cols));
        }

        Selected = seats.FirstOrDefault(s => s.Id == Seat && s.IsActive && !Busy.Contains(s.Id));
        if (Selected != null)
        {
            var price = await api.GetPriceAsync(Selected.Id, startUtc, Hours);
            Price = price.Value?.Price;
            PriceError = price.Error;
        }
    }

    public async Task<IActionResult> OnPostBookAsync()
    {
        Normalize();
        if (!User.IsInRole("client"))
        {
            return Challenge();
        }

        var result = await api.CreateBookingAsync(Seat ?? 0, time.ToUtc(StartLocal), Hours);
        if (result.Value == null)
        {
            TempData["Error"] = result.Error;
            return RedirectToPage(new { Date = Date!.Value.ToString("yyyy-MM-dd"), Time, Hours, Seat });
        }

        TempData["Message"] = $"Место {result.Value.SeatNumber} забронировано на {StartLocal:dd.MM.yyyy} с {StartLocal:HH:mm} до {EndLocal:HH:mm}, цена {result.Value.Price:0} ₽";
        return RedirectToPage("/Bookings/My");
    }

    private void Normalize()
    {
        var now = time.Now;
        if (Hours is < 1 or > 5)
        {
            Hours = 2;
        }

        if (Date == null || !TimeSpan.TryParse(Time, out var start) || start.Minutes % 30 != 0 || start >= TimeSpan.FromDays(1))
        {
            var next = now.Date.AddMinutes(Math.Ceiling(now.TimeOfDay.TotalMinutes / 30) * 30);
            Date = next.Date;
            start = next.TimeOfDay;
        }

        Time = start.ToString(@"hh\:mm");
        StartLocal = Date.Value.Date + start;
    }
}
