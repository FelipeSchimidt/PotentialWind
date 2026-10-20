using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;

namespace PotentialWind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WindFarmsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WindFarmsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetWindFarms()
    {
        var farms = await _context.WindFarms
            .Include(f => f.SelectedTurbine)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                f.Location,
                f.Latitude,
                f.Longitude,
                f.AltitudeM,
                f.RoughnessExponent,
                f.AirDensityKgM3,
                f.TurbineCount,
                f.TurbineSpacingD,
                f.WakeLossPercentage,
                f.ElectricalLossPercentage,
                f.AvailabilityLossPercentage,
                f.OtherLossesPercentage,
                f.EnergyPriceMWh,
                f.CapexPerMW,
                f.OpexAnnualPercentCapex,
                f.DiscountRatePercent,
                f.ProjectLifespanYears,
                f.SelectedTurbineId,
                Turbine = f.SelectedTurbine == null ? null : new
                {
                    f.SelectedTurbine.Id,
                    f.SelectedTurbine.Manufacturer,
                    f.SelectedTurbine.ModelName,
                    f.SelectedTurbine.RatedPowerKW,
                    f.SelectedTurbine.RotorDiameterM,
                    f.SelectedTurbine.HubHeightM
                },
                TotalCapacityMW = f.SelectedTurbine == null ? 0 : (f.SelectedTurbine.RatedPowerKW / 1000.0) * f.TurbineCount,
                MeasurementsCount = f.Measurements.Count(),
                LatestSimulation = f.SimulationResults.OrderByDescending(s => s.CalculatedAt).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(farms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetWindFarm(int id)
    {
        var farm = await _context.WindFarms
            .Include(f => f.SelectedTurbine)
            .Include(f => f.SimulationResults)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (farm == null)
        {
            return NotFound(new { message = $"Parque eólico #{id} não encontrado." });
        }

        return Ok(new
        {
            farm.Id,
            farm.Name,
            farm.Description,
            farm.Location,
            farm.Latitude,
            farm.Longitude,
            farm.AltitudeM,
            farm.RoughnessExponent,
            farm.RoughnessLengthZ0,
            farm.AirDensityKgM3,
            farm.TurbineCount,
            farm.TurbineSpacingD,
            farm.WakeLossPercentage,
            farm.ElectricalLossPercentage,
            farm.AvailabilityLossPercentage,
            farm.OtherLossesPercentage,
            farm.EnergyPriceMWh,
            farm.CapexPerMW,
            farm.OpexAnnualPercentCapex,
            farm.DiscountRatePercent,
            farm.ProjectLifespanYears,
            farm.SelectedTurbineId,
            farm.SelectedTurbine,
            TotalCapacityMW = farm.SelectedTurbine == null ? 0 : (farm.SelectedTurbine.RatedPowerKW / 1000.0) * farm.TurbineCount,
            MeasurementsCount = await _context.WindMeasurements.CountAsync(m => m.WindFarmId == id),
            RecentSimulations = farm.SimulationResults.OrderByDescending(s => s.CalculatedAt).Take(5).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<WindFarm>> CreateWindFarm([FromBody] WindFarm farm)
    {
        farm.CreatedAt = DateTime.UtcNow;
        _context.WindFarms.Add(farm);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWindFarm), new { id = farm.Id }, farm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWindFarm(int id, [FromBody] WindFarm updated)
    {
        var existing = await _context.WindFarms.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = $"Parque eólico #{id} não encontrado." });
        }

        existing.Name = updated.Name;
        existing.Description = updated.Description;
        existing.Location = updated.Location;
        existing.Latitude = updated.Latitude;
        existing.Longitude = updated.Longitude;
        existing.AltitudeM = updated.AltitudeM;
        existing.RoughnessExponent = updated.RoughnessExponent;
        existing.RoughnessLengthZ0 = updated.RoughnessLengthZ0;
        existing.AirDensityKgM3 = updated.AirDensityKgM3;
        existing.SelectedTurbineId = updated.SelectedTurbineId;
        existing.TurbineCount = updated.TurbineCount;
        existing.TurbineSpacingD = updated.TurbineSpacingD;
        existing.WakeLossPercentage = updated.WakeLossPercentage;
        existing.ElectricalLossPercentage = updated.ElectricalLossPercentage;
        existing.AvailabilityLossPercentage = updated.AvailabilityLossPercentage;
        existing.OtherLossesPercentage = updated.OtherLossesPercentage;
        existing.EnergyPriceMWh = updated.EnergyPriceMWh;
        existing.CapexPerMW = updated.CapexPerMW;
        existing.OpexAnnualPercentCapex = updated.OpexAnnualPercentCapex;
        existing.DiscountRatePercent = updated.DiscountRatePercent;
        existing.ProjectLifespanYears = updated.ProjectLifespanYears;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWindFarm(int id)
    {
        var farm = await _context.WindFarms.FindAsync(id);
        if (farm == null)
        {
            return NotFound(new { message = $"Parque eólico #{id} não encontrado." });
        }

        _context.WindFarms.Remove(farm);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/simulations")]
    public async Task<ActionResult<IEnumerable<WindSimulationResult>>> GetFarmSimulations(int id)
    {
        var simulations = await _context.WindSimulationResults
            .Where(s => s.WindFarmId == id)
            .OrderByDescending(s => s.CalculatedAt)
            .ToListAsync();

        return Ok(simulations);
    }
}
