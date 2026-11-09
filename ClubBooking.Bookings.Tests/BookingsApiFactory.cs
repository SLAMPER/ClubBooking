using System.Net.Http.Headers;
using ClubBooking.Bookings.Models;
using ClubBooking.Bookings.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ClubBooking.Bookings.Tests;

public class BookingsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    public FakeSeatsClient Seats { get; } = new();

    public Task InitializeAsync() => _postgres.StartAsync();

    public new Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    public HttpClient CreateClient(Guid userId, string role = "client")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.Create(userId, role));
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", TestTokens.Key);
        builder.ConfigureServices(services => services.AddSingleton<ISeatsClient>(Seats));
    }
}

public class FakeSeatsClient : ISeatsClient
{
    private readonly Dictionary<int, SeatInfo> _seats = new();

    public void Add(int id, bool isActive = true) =>
        _seats[id] = new SeatInfo(id, $"S{id}", isActive, "Обычный зал", 120, 80);

    public Task<SeatInfo?> GetSeatAsync(int id) => Task.FromResult(_seats.GetValueOrDefault(id));
}
