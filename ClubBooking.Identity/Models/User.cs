namespace ClubBooking.Identity.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = Roles.Client;
    public DateTime CreatedAt { get; set; }
}

public static class Roles
{
    public const string Client = "client";
    public const string Admin = "admin";
}
