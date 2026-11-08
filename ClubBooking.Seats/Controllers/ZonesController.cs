using ClubBooking.Seats.Data;
using ClubBooking.Seats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBooking.Seats.Controllers;

[ApiController]
[Route("api/zones")]
public class ZonesController(SeatsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<ZoneDto>> GetAll()
    {
        return await db.Zones
            .OrderBy(z => z.Id)
            .Select(z => new ZoneDto(z.Id, z.Name, z.DayRate, z.NightRate))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ZoneDto>> Create(ZoneRequest request)
    {
        if (await db.Zones.AnyAsync(z => z.Name == request.Name))
        {
            return Conflict("Зона с таким названием уже есть");
        }

        var zone = new Zone { Name = request.Name, DayRate = request.DayRate, NightRate = request.NightRate };
        db.Zones.Add(zone);
        await db.SaveChangesAsync();

        return Created($"/api/zones/{zone.Id}", new ZoneDto(zone.Id, zone.Name, zone.DayRate, zone.NightRate));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, ZoneRequest request)
    {
        var zone = await db.Zones.FindAsync(id);
        if (zone == null)
        {
            return NotFound();
        }

        if (await db.Zones.AnyAsync(z => z.Name == request.Name && z.Id != id))
        {
            return Conflict("Зона с таким названием уже есть");
        }

        zone.Name = request.Name;
        zone.DayRate = request.DayRate;
        zone.NightRate = request.NightRate;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var zone = await db.Zones.FindAsync(id);
        if (zone == null)
        {
            return NotFound();
        }

        if (await db.Seats.AnyAsync(s => s.ZoneId == id))
        {
            return Conflict("В зоне есть места, сначала удалите или перенесите их");
        }

        db.Zones.Remove(zone);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
