namespace ClubBooking.Bookings.Models;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = "";
    public int SeatId { get; set; }
    public string SeatNumber { get; set; } = "";
    public string ZoneName { get; set; } = "";
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal Price { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
}

public enum BookingStatus
{
    Booked,
    CheckedIn,
    Cancelled,
    NoShow
}
