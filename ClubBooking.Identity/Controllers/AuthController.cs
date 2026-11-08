using ClubBooking.Identity.Data;
using ClubBooking.Identity.Models;
using ClubBooking.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Identity.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UsersDbContext db, IPasswordHasher<User> hasher, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict("Пользователь с таким email уже есть");
        }

        var user = new User { Id = Guid.NewGuid(), Email = email, Role = Roles.Client, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return tokens.Create(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Неверный email или пароль");
        }

        return tokens.Create(user);
    }
}
