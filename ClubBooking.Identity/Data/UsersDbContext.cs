using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Identity.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
}
