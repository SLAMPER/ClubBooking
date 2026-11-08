using ClubBooking.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Identity.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Email).HasMaxLength(100);
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Role).HasMaxLength(20);
        });
    }
}
