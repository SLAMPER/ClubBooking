using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages.Admin;

public class TodayModel(ClubApi api, ClubTime time) : PageModel
{
    public List<BookingDto> Bookings { get; private set; } = [];
    public ClubTime Time => time;

    public async Task OnGetAsync()
    {
        Bookings = await api.GetTodayAsync();
    }

    public async Task<IActionResult> OnPostCheckInAsync(Guid id)
    {
        var result = await api.CheckInAsync(id);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? "Приход отмечен" : result.Error;
        return RedirectToPage();
    }
}
