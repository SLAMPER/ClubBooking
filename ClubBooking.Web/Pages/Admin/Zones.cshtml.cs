using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages.Admin;

public class ZonesModel(ClubApi api) : PageModel
{
    public List<ZoneDto> Zones { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Zones = await api.GetZonesAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync(int? id, string name, decimal dayRate, decimal nightRate)
    {
        var result = await api.SaveZoneAsync(id, name, dayRate, nightRate);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? "Зона сохранена" : result.Error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteZoneAsync(id);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? "Зона удалена" : result.Error;
        return RedirectToPage();
    }
}
