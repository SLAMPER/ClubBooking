namespace ClubBooking.Seats.Models;

public class Seat
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public string Number { get; set; } = "";
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsActive { get; set; } = true;
}
