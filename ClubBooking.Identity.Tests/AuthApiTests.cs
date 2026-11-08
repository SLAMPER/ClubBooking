using System.Net;
using System.Net.Http.Json;
using ClubBooking.Identity.Models;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ClubBooking.Identity.Tests;

public class AuthApiTests(IdentityApiFactory factory) : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ThenLogin_ReturnsTokenWithClientRole()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Anna@Mail.ru", "secret1"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("anna@mail.ru", "secret1"));
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(result!.Token);
        Assert.Equal("client", token.GetClaim("role").Value);
        Assert.Equal("anna@mail.ru", token.GetClaim("email").Value);
    }

    [Fact]
    public async Task Register_WithTakenEmail_ReturnsConflict()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("twice@mail.ru", "secret1"));

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("TWICE@mail.ru", "secret2"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("short@mail.ru", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("wrong@mail.ru", "secret1"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("wrong@mail.ru", "secret2"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AsSeededAdmin_ReturnsAdminRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@club.local", "admin123"));
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal("admin", result!.Role);
    }
}
