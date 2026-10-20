using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;
using PotentialWind.Api.Services;

namespace PotentialWind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WindMeasurementsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WindMeasurementsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WindMeasurement>>> GetMeasurements([FromQuery] int windFarmId, [FromQuery] int limit = 100)
    {
        var query = _context.WindMeasurements.Where(m => m.WindFarmId == windFarmId);

        var list = await query
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int windFarmId)
    {
        var measurements = await _context.WindMeasurements
            .Where(m => m.WindFarmId == windFarmId)
            .ToListAsync();

        if (!measurements.Any())
        {
            return Ok(new
            {
                totalRecords = 0,
                message = "Nenhuma medição anemométrica encontrada para este parque."
            });
        }

        double count = measurements.Count;
        double meanSpeed = measurements.Average(m => m.WindSpeed);
        double minSpeed = measurements.Min(m => m.WindSpeed);
        double maxSpeed = measurements.Max(m => m.WindSpeed);

        double sumSquares = measurements.Sum(m => Math.Pow(m.WindSpeed - meanSpeed, 2));
        double stdDev = Math.Sqrt(sumSquares / count);

        var (k, c) = WeibullCalculator.EstimateFromMeanAndStdDev(meanSpeed, stdDev);
        double powerDensity = WeibullCalculator.CalculatePowerDensity(k, c);
        var classification = WeibullCalculator.ClassifyWindResource(powerDensity, meanSpeed);

        // Rosa dos ventos (Wind Rose) - 16 setores de 22.5° cada
        string[] sectors = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        var windRose = new List<object>();

        for (int i = 0; i < 16; i++)
        {
            double center = i * 22.5;
            double low = (center - 11.25 + 360.0) % 360.0;
            double high = (center + 11.25) % 360.0;

            var inSector = measurements.Where(m =>
            {
                if (low > high) // Atravessa 360° (Norte)
                    return m.WindDirectionDeg >= low || m.WindDirectionDeg < high;
                return m.WindDirectionDeg >= low && m.WindDirectionDeg < high;
            }).ToList();

            double sectorPercentage = (inSector.Count / count) * 100.0;
            double sectorAvgSpeed = inSector.Any() ? inSector.Average(m => m.WindSpeed) : 0.0;

            windRose.Add(new
            {
                sector = sectors[i],
                angle = center,
                frequencyPercent = Math.Round(sectorPercentage, 2),
                averageSpeed = Math.Round(sectorAvgSpeed, 2),
                count = inSector.Count
            });
        }

        return Ok(new
        {
            totalRecords = count,
            meanSpeed = Math.Round(meanSpeed, 2),
            minSpeed = Math.Round(minSpeed, 2),
            maxSpeed = Math.Round(maxSpeed, 2),
            stdDev = Math.Round(stdDev, 2),
            weibullK = Math.Round(k, 3),
            weibullC = Math.Round(c, 3),
            powerDensity = Math.Round(powerDensity, 1),
            windClass = classification.WindClass,
            recommendation = classification.Recommendation,
            windRose
        });
    }

    [HttpPost]
    public async Task<ActionResult<WindMeasurement>> AddMeasurement([FromBody] WindMeasurement measurement)
    {
        _context.WindMeasurements.Add(measurement);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMeasurements), new { windFarmId = measurement.WindFarmId }, measurement);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchImport([FromBody] BatchMeasurementsImportDto dto)
    {
        if (dto.Points == null || !dto.Points.Any())
        {
            return BadRequest(new { message = "Nenhum ponto de medição fornecido." });
        }

        var measurements = dto.Points.Select(p => new WindMeasurement
        {
            WindFarmId = dto.WindFarmId,
            Timestamp = p.Timestamp,
            WindSpeed = p.WindSpeed,
            WindDirectionDeg = p.WindDirectionDeg,
            MeasurementHeightM = dto.MeasurementHeightM,
            TemperatureC = p.TemperatureC,
            PressureHPa = p.PressureHPa
        }).ToList();

        await _context.WindMeasurements.AddRangeAsync(measurements);
        await _context.SaveChangesAsync();

        return Ok(new { importedCount = measurements.Count, message = $"{measurements.Count} medições importadas com sucesso." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeasurement(int id)
    {
        var measurement = await _context.WindMeasurements.FindAsync(id);
        if (measurement == null)
        {
            return NotFound(new { message = $"Medição #{id} não encontrada." });
        }

        _context.WindMeasurements.Remove(measurement);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("by-farm/{farmId}")]
    public async Task<IActionResult> ClearFarmMeasurements(int farmId)
    {
        var items = await _context.WindMeasurements.Where(m => m.WindFarmId == farmId).ToListAsync();
        _context.WindMeasurements.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{items.Count} medições removidas do parque #{farmId}." });
    }
}
