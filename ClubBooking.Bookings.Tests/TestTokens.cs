using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClubBooking.Bookings.Tests;

public static class TestTokens
{
    public const string Key = "club-booking-test-key-0123456789-abcdefgh";

    public static string Create(Guid userId, string role)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "ClubBooking",
            Subject = new ClaimsIdentity([new Claim("sub", userId.ToString()), new Claim("email", $"{userId:N}@test.local"), new Claim("role", role)]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
