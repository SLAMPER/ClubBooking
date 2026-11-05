using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Seats.Data;

public class SeatsDbContext(DbContextOptions<SeatsDbContext> options) : DbContext(options)
{
}
