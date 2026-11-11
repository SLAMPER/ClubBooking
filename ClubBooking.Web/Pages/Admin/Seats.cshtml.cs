using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages.Admin;

public class SeatsModel(ClubApi api) : PageModel
{
    public List<SeatDto> Seats { get; private set; } = [];
    public List<ZoneDto> Zones { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Seats = await api.GetSeatsAsync();
        Zones = await api.GetZonesAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync(int? id, int zoneId, string number, int row, int col)
    {
        var result = await api.SaveSeatAsync(id, zoneId, number, row, col);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? "Место сохранено" : result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActiveAsync(int id, bool isActive)
    {
        var result = await api.SetSeatActiveAsync(id, isActive);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? (isActive ? "Место открыто" : "Место закрыто на ремонт") : result.Error;
        return RedirectToPage();
    }
}
