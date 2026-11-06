using ClubBooking.Seats.Models;

namespace ClubBooking.Seats.Data;

public static class SeedData
{
    public static void Seed(SeatsDbContext db)
    {
        if (db.Zones.Any())
        {
            return;
        }

        var regular = new Zone { Name = "Обычный зал", DayRate = 120, NightRate = 80 };
        var vip = new Zone { Name = "VIP", DayRate = 250, NightRate = 150 };

        for (var i = 1; i <= 10; i++)
        {
            regular.Seats.Add(new Seat { Number = i.ToString("00"), Row = (i - 1) / 5 + 1, Col = (i - 1) % 5 + 1 });
        }

        for (var i = 1; i <= 4; i++)
        {
            vip.Seats.Add(new Seat { Number = $"V{i}", Row = 1, Col = i });
        }

        db.Zones.AddRange(regular, vip);
        db.SaveChanges();
    }
}
