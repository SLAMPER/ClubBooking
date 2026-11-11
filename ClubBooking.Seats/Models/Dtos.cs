using System.ComponentModel.DataAnnotations;

namespace ClubBooking.Seats.Models;

public record ZoneRequest(
    [Required(ErrorMessage = "Введите название зоны"), StringLength(50, ErrorMessage = "Название не длиннее 50 символов")] string Name,
    [Range(0, 100000, ErrorMessage = "Тариф должен быть от 0 до 100000")] decimal DayRate,
    [Range(0, 100000, ErrorMessage = "Тариф должен быть от 0 до 100000")] decimal NightRate);

public record ZoneDto(int Id, string Name, decimal DayRate, decimal NightRate);

public record SeatRequest(
    int ZoneId,
    [Required(ErrorMessage = "Введите номер места"), StringLength(10, ErrorMessage = "Номер не длиннее 10 символов")] string Number,
    [Range(1, 20, ErrorMessage = "Ряд должен быть от 1 до 20")] int Row,
    [Range(1, 20, ErrorMessage = "Колонка должна быть от 1 до 20")] int Col);

public record SeatDto(
    int Id,
    string Number,
    int Row,
    int Col,
    bool IsActive,
    int ZoneId,
    string ZoneName,
    decimal DayRate,
    decimal NightRate);
