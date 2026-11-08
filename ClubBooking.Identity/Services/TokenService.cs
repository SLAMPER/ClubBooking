using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClubBooking.Identity.Models;
using Microsoft.IdentityModel.Tokens;

namespace ClubBooking.Identity.Services;

public class TokenService(IConfiguration configuration)
{
    public TokenResponse Create(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            claims:
            [
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("role", user.Role)
            ],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Email, user.Role, expiresAt);
    }
}
