using System.Net;
using ClubBooking.Bookings.Models;

namespace ClubBooking.Bookings.Services;

public interface ISeatsClient
{
    Task<SeatInfo?> GetSeatAsync(int id);
}

public class SeatsClient(HttpClient http) : ISeatsClient
{
    public async Task<SeatInfo?> GetSeatAsync(int id)
    {
        var response = await http.GetAsync($"/api/seats/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SeatInfo>();
    }
}
