using ClubBooking.Bookings.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Bookings.Data;

public class BookingsDbContext(DbContextOptions<BookingsDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.Property(b => b.UserEmail).HasMaxLength(100);
            booking.Property(b => b.SeatNumber).HasMaxLength(10);
            booking.Property(b => b.ZoneName).HasMaxLength(50);
            booking.Property(b => b.Price).HasPrecision(10, 2);
            booking.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            booking.HasIndex(b => new { b.SeatId, b.StartUtc });
            booking.HasIndex(b => b.UserId);
        });
    }
}
