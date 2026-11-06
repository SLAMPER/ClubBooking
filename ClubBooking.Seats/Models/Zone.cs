namespace ClubBooking.Seats.Models;

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal DayRate { get; set; }
    public decimal NightRate { get; set; }
    public List<Seat> Seats { get; set; } = [];
}
