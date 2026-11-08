using System.ComponentModel.DataAnnotations;

namespace ClubBooking.Identity.Models;

public record RegisterRequest(
    [Required, EmailAddress, StringLength(100)] string Email,
    [Required, StringLength(100, MinimumLength = 6)] string Password);

public record LoginRequest([Required] string Email, [Required] string Password);

public record TokenResponse(string Token, string Email, string Role, DateTime ExpiresAt);
