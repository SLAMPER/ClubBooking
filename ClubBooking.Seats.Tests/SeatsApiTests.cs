using System.Net;
using System.Net.Http.Json;
using ClubBooking.Seats.Models;

namespace ClubBooking.Seats.Tests;

public class SeatsApiTests(SeatsApiFactory factory) : IClassFixture<SeatsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient("admin");

    [Fact]
    public async Task GetSeats_ReturnsSeededSeatsWithZoneRates()
    {
        var seats = await _client.GetFromJsonAsync<List<SeatDto>>("/api/seats");

        var vip = Assert.Single(seats!, s => s.Number == "V1");
        Assert.Equal("VIP", vip.ZoneName);
        Assert.Equal(250, vip.DayRate);
        Assert.Equal(150, vip.NightRate);
    }

    [Fact]
    public async Task CreateSeat_InUnknownZone_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/seats", new SeatRequest(999, "X1", 1, 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSeat_WithTakenNumber_ReturnsConflict()
    {
        var zone = await CreateZoneAsync("Номера");

        var response = await _client.PostAsJsonAsync("/api/seats", new SeatRequest(zone.Id, "01", 1, 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateSeat_OnTakenCell_ReturnsConflict()
    {
        var zone = await CreateZoneAsync("Клетки");
        await _client.PostAsJsonAsync("/api/seats", new SeatRequest(zone.Id, "K1", 1, 1));

        var response = await _client.PostAsJsonAsync("/api/seats", new SeatRequest(zone.Id, "K2", 1, 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteZone_WithSeats_ReturnsConflict()
    {
        var zone = await CreateZoneAsync("Непустая");
        await _client.PostAsJsonAsync("/api/seats", new SeatRequest(zone.Id, "N1", 1, 1));

        var response = await _client.DeleteAsync($"/api/zones/{zone.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CloseSeat_MakesItInactive_AndOpenMakesItActiveAgain()
    {
        var zone = await CreateZoneAsync("Ремонт");
        var created = await _client.PostAsJsonAsync("/api/seats", new SeatRequest(zone.Id, "R1", 1, 1));
        var seat = await created.Content.ReadFromJsonAsync<SeatDto>();

        await _client.PostAsync($"/api/seats/{seat!.Id}/close", null);
        var closed = await _client.GetFromJsonAsync<SeatDto>($"/api/seats/{seat.Id}");
        await _client.PostAsync($"/api/seats/{seat.Id}/open", null);
        var opened = await _client.GetFromJsonAsync<SeatDto>($"/api/seats/{seat.Id}");

        Assert.False(closed!.IsActive);
        Assert.True(opened!.IsActive);
    }

    [Fact]
    public async Task CreateZone_WithoutToken_ReturnsUnauthorized()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/zones", new ZoneRequest("Без входа", 100, 60));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CloseSeat_AsClient_ReturnsForbidden()
    {
        var response = await factory.CreateClient("client").PostAsync("/api/seats/1/close", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<ZoneDto> CreateZoneAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/zones", new ZoneRequest(name, 100, 60));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ZoneDto>())!;
    }
}
