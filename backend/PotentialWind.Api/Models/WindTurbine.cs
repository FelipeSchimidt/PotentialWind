using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotentialWind.Api.Models;

public class WindTurbine
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Potência nominal do gerador em quilowatts (kW)
    /// </summary>
    public double RatedPowerKW { get; set; }

    /// <summary>
    /// Diâmetro do rotor em metros (m)
    /// </summary>
    public double RotorDiameterM { get; set; }

    /// <summary>
    /// Altura recomendada da torre / cubo do rotor em metros (m)
    /// </summary>
    public double HubHeightM { get; set; }

    /// <summary>
    /// Velocidade de partida / início de geração (cut-in) em m/s
    /// </summary>
    public double CutInSpeed { get; set; }

    /// <summary>
    /// Velocidade para atingir a potência nominal (rated speed) em m/s
    /// </summary>
    public double RatedSpeed { get; set; }

    /// <summary>
    /// Velocidade de corte / segurança por vento forte (cut-out) em m/s
    /// </summary>
    public double CutOutSpeed { get; set; }

    /// <summary>
    /// Curva de potência serializada em JSON: array de { "windSpeed": double, "powerKW": double }
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string PowerCurveJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Área de varredura do rotor (m²) = pi * (D/2)^2
    /// </summary>
    [NotMapped]
    public double SweptAreaM2 => Math.PI * Math.Pow(RotorDiameterM / 2.0, 2);
}
