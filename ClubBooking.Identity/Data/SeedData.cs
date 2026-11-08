using ClubBooking.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace ClubBooking.Identity.Data;

public static class SeedData
{
    public static void Seed(UsersDbContext db, IPasswordHasher<User> hasher)
    {
        if (db.Users.Any())
        {
            return;
        }

        AddUser(db, hasher, "admin@club.local", "admin123", Roles.Admin);
        AddUser(db, hasher, "client@club.local", "client123", Roles.Client);
        db.SaveChanges();
    }

    private static void AddUser(UsersDbContext db, IPasswordHasher<User> hasher, string email, string password, string role)
    {
        var user = new User { Id = Guid.NewGuid(), Email = email, Role = role, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
    }
}
