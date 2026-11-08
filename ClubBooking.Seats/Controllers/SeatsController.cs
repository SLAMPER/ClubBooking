using ClubBooking.Seats.Data;
using ClubBooking.Seats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Seats.Controllers;

[ApiController]
[Route("api/seats")]
public class SeatsController(SeatsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<SeatDto>> GetAll()
    {
        return await db.Seats
            .OrderBy(s => s.ZoneId).ThenBy(s => s.Row).ThenBy(s => s.Col)
            .Select(s => new SeatDto(s.Id, s.Number, s.Row, s.Col, s.IsActive, s.ZoneId, s.Zone!.Name, s.Zone.DayRate, s.Zone.NightRate))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SeatDto>> Get(int id)
    {
        var seat = await db.Seats
            .Where(s => s.Id == id)
            .Select(s => new SeatDto(s.Id, s.Number, s.Row, s.Col, s.IsActive, s.ZoneId, s.Zone!.Name, s.Zone.DayRate, s.Zone.NightRate))
            .FirstOrDefaultAsync();

        return seat == null ? NotFound() : seat;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SeatDto>> Create(SeatRequest request)
    {
        var error = await CheckAsync(request, null);
        if (error != null)
        {
            return error;
        }

        var seat = new Seat { ZoneId = request.ZoneId, Number = request.Number, Row = request.Row, Col = request.Col };
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = seat.Id }, (await Get(seat.Id)).Value);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, SeatRequest request)
    {
        var seat = await db.Seats.FindAsync(id);
        if (seat == null)
        {
            return NotFound();
        }

        var error = await CheckAsync(request, id);
        if (error != null)
        {
            return error;
        }

        seat.ZoneId = request.ZoneId;
        seat.Number = request.Number;
        seat.Row = request.Row;
        seat.Col = request.Col;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var seat = await db.Seats.FindAsync(id);
        if (seat == null)
        {
            return NotFound();
        }

        db.Seats.Remove(seat);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "admin")]
    public Task<IActionResult> Close(int id) => SetActiveAsync(id, false);

    [HttpPost("{id}/open")]
    [Authorize(Roles = "admin")]
    public Task<IActionResult> Open(int id) => SetActiveAsync(id, true);

    private async Task<IActionResult> SetActiveAsync(int id, bool isActive)
    {
        var seat = await db.Seats.FindAsync(id);
        if (seat == null)
        {
            return NotFound();
        }

        seat.IsActive = isActive;
        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> CheckAsync(SeatRequest request, int? id)
    {
        if (!await db.Zones.AnyAsync(z => z.Id == request.ZoneId))
        {
            return BadRequest("Такой зоны нет");
        }

        if (await db.Seats.AnyAsync(s => s.Number == request.Number && s.Id != id))
        {
            return Conflict("Место с таким номером уже есть");
        }

        if (await db.Seats.AnyAsync(s => s.ZoneId == request.ZoneId && s.Row == request.Row && s.Col == request.Col && s.Id != id))
        {
            return Conflict("Эта клетка на карте уже занята другим местом");
        }

        return null;
    }
}
