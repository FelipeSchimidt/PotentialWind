using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotentialWind.Api.Models;

public class WindFarm
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Altitude média do terreno do parque (m)
    /// </summary>
    public double AltitudeM { get; set; } = 300.0;

    /// <summary>
    /// Expoente de cisalhamento do vento (Lei de Potência de Hellmann - alpha)
    /// Típico: 0.10 (mar), 0.14 (pastagem/terreno plano), 0.20 (bosques/obstáculos)
    /// </summary>
    public double RoughnessExponent { get; set; } = 0.14;

    /// <summary>
    /// Comprimento de rugosidade do solo z0 (m) para perfil logarítmico
    /// </summary>
    public double RoughnessLengthZ0 { get; set; } = 0.03;

    /// <summary>
    /// Densidade do ar local rho (kg/m³). Padrão nível do mar = 1.225 kg/m³
    /// </summary>
    public double AirDensityKgM3 { get; set; } = 1.225;

    public int? SelectedTurbineId { get; set; }

    [ForeignKey(nameof(SelectedTurbineId))]
    public WindTurbine? SelectedTurbine { get; set; }

    /// <summary>
    /// Número de turbinas eólicas planejadas para o parque
    /// </summary>
    public int TurbineCount { get; set; } = 10;

    /// <summary>
    /// Espaçamento entre turbinas em múltiplos do diâmetro do rotor (ex: 5D a 7D)
    /// </summary>
    public double TurbineSpacingD { get; set; } = 5.0;

    /// <summary>
    /// Perdas aerodinâmicas por efeito esteira (Wake Effect Losses) em porcentagem
    /// </summary>
    public double WakeLossPercentage { get; set; } = 7.5;

    /// <summary>
    /// Perdas elétricas (cabos, transformadores e subestações) em porcentagem
    /// </summary>
    public double ElectricalLossPercentage { get; set; } = 2.5;

    /// <summary>
    /// Indisponibilidade técnica / manutenção em porcentagem
    /// </summary>
    public double AvailabilityLossPercentage { get; set; } = 3.0;

    /// <summary>
    /// Outras perdas (degradação aerodinâmica, sujeira, gelo, restrições da rede) em porcentagem
    /// </summary>
    public double OtherLossesPercentage { get; set; } = 2.0;

    /// <summary>
    /// Preço de venda da energia acordado em PPA / leilão (R$/MWh)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal EnergyPriceMWh { get; set; } = 220.00m;

    /// <summary>
    /// Investimento inicial estimado por MW instalado (CAPEX em R$/MW)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CapexPerMW { get; set; } = 5500000.00m;

    /// <summary>
    /// Custo anual de operação e manutenção como % do CAPEX inicial
    /// </summary>
    public double OpexAnnualPercentCapex { get; set; } = 2.5;

    /// <summary>
    /// Taxa de desconto anual para análise de viabilidade e LCOE (%)
    /// </summary>
    public double DiscountRatePercent { get; set; } = 8.5;

    /// <summary>
    /// Vida útil estimada do projeto eólico (anos)
    /// </summary>
    public int ProjectLifespanYears { get; set; } = 25;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<WindMeasurement> Measurements { get; set; } = new List<WindMeasurement>();
    public ICollection<WindSimulationResult> SimulationResults { get; set; } = new List<WindSimulationResult>();
}
