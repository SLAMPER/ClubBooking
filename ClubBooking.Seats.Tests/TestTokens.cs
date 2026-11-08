using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClubBooking.Seats.Tests;

public static class TestTokens
{
    public const string Key = "club-booking-test-key-0123456789-abcdefgh";

    public static string Create(string role)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "ClubBooking",
            Subject = new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString()), new Claim("email", $"{role}@test.local"), new Claim("role", role)]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
