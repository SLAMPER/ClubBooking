using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages.Bookings;

public class MyModel(ClubApi api, ClubTime time) : PageModel
{
    public List<BookingDto> Bookings { get; private set; } = [];
    public ClubTime Time => time;

    public async Task OnGetAsync()
    {
        Bookings = await api.GetMyBookingsAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var result = await api.CancelBookingAsync(id);
        TempData[result.Ok ? "Message" : "Error"] = result.Ok ? "Бронь отменена" : result.Error;
        return RedirectToPage();
    }
}
