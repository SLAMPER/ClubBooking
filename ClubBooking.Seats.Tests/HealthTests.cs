using System.Net;

namespace ClubBooking.Seats.Tests;

public class HealthTests(SeatsApiFactory factory) : IClassFixture<SeatsApiFactory>
{
    [Fact]
    public async Task Health_ReturnsHealthy_WhenDatabaseIsAvailable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
