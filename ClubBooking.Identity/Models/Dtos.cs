using System.ComponentModel.DataAnnotations;

namespace ClubBooking.Identity.Models;

public record RegisterRequest(
    [Required(ErrorMessage = "Введите email"), EmailAddress(ErrorMessage = "Неверный формат email"), StringLength(100, ErrorMessage = "Email не длиннее 100 символов")] string Email,
    [Required(ErrorMessage = "Введите пароль"), StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче 6 символов")] string Password);

public record LoginRequest(
    [Required(ErrorMessage = "Введите email")] string Email,
    [Required(ErrorMessage = "Введите пароль")] string Password);

public record TokenResponse(string Token, string Email, string Role, DateTime ExpiresAt);
