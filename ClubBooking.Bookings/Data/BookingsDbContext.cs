using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Bookings.Data;

public class BookingsDbContext(DbContextOptions<BookingsDbContext> options) : DbContext(options)
{
}
