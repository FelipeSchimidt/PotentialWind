using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotentialWind.Api.Models;

public class WindMeasurement
{
    [Key]
    public int Id { get; set; }

    public int WindFarmId { get; set; }

    [ForeignKey(nameof(WindFarmId))]
    public WindFarm? WindFarm { get; set; }

    /// <summary>
    /// Data e hora da medição anemométrica
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Velocidade do vento observada (m/s)
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// Direção do vento em graus (0° a 360°)
    /// </summary>
    public double WindDirectionDeg { get; set; }

    /// <summary>
    /// Altura do anemômetro na torre de medição (m)
    /// </summary>
    public double MeasurementHeightM { get; set; } = 50.0;

    /// <summary>
    /// Temperatura ambiente (°C)
    /// </summary>
    public double? TemperatureC { get; set; } = 25.0;

    /// <summary>
    /// Pressão atmosférica (hPa)
    /// </summary>
    public double? PressureHPa { get; set; } = 1013.25;
}
