using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;

namespace PotentialWind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WindTurbinesController : ControllerBase
{
    private readonly AppDbContext _context;

    public WindTurbinesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WindTurbine>>> GetTurbines()
    {
        var list = await _context.WindTurbines
            .OrderBy(t => t.Manufacturer)
            .ThenBy(t => t.ModelName)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WindTurbine>> GetTurbine(int id)
    {
        var turbine = await _context.WindTurbines.FindAsync(id);
        if (turbine == null)
        {
            return NotFound(new { message = $"Aerogerador #{id} não encontrado." });
        }

        return Ok(turbine);
    }

    [HttpPost]
    public async Task<ActionResult<WindTurbine>> CreateTurbine([FromBody] WindTurbine turbine)
    {
        turbine.CreatedAt = DateTime.UtcNow;
        _context.WindTurbines.Add(turbine);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTurbine), new { id = turbine.Id }, turbine);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTurbine(int id, [FromBody] WindTurbine updated)
    {
        var existing = await _context.WindTurbines.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = $"Aerogerador #{id} não encontrado." });
        }

        existing.Manufacturer = updated.Manufacturer;
        existing.ModelName = updated.ModelName;
        existing.RatedPowerKW = updated.RatedPowerKW;
        existing.RotorDiameterM = updated.RotorDiameterM;
        existing.HubHeightM = updated.HubHeightM;
        existing.CutInSpeed = updated.CutInSpeed;
        existing.RatedSpeed = updated.RatedSpeed;
        existing.CutOutSpeed = updated.CutOutSpeed;
        existing.PowerCurveJson = updated.PowerCurveJson;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTurbine(int id)
    {
        var turbine = await _context.WindTurbines.FindAsync(id);
        if (turbine == null)
        {
            return NotFound(new { message = $"Aerogerador #{id} não encontrado." });
        }

        _context.WindTurbines.Remove(turbine);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
