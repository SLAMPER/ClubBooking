using ClubBooking.Seats.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Seats.Data;

public class SeatsDbContext(DbContextOptions<SeatsDbContext> options) : DbContext(options)
{
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Seat> Seats => Set<Seat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Zone>(zone =>
        {
            zone.Property(z => z.Name).HasMaxLength(50);
            zone.HasIndex(z => z.Name).IsUnique();
            zone.Property(z => z.DayRate).HasPrecision(10, 2);
            zone.Property(z => z.NightRate).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Seat>(seat =>
        {
            seat.Property(s => s.Number).HasMaxLength(10);
            seat.HasIndex(s => s.Number).IsUnique();
            seat.HasIndex(s => new { s.ZoneId, s.Row, s.Col }).IsUnique();
            seat.HasOne(s => s.Zone).WithMany(z => z.Seats).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
