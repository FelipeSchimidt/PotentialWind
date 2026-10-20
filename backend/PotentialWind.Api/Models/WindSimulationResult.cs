using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotentialWind.Api.Models;

public class WindSimulationResult
{
    [Key]
    public int Id { get; set; }

    public int WindFarmId { get; set; }

    [ForeignKey(nameof(WindFarmId))]
    public WindFarm? WindFarm { get; set; }

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Velocidade média do vento na altura do cubo (Hub Height) em m/s
    /// </summary>
    public double AverageWindSpeedAtHub { get; set; }

    /// <summary>
    /// Parâmetro de forma de Weibull (k)
    /// </summary>
    public double WeibullK { get; set; }

    /// <summary>
    /// Parâmetro de escala de Weibull (c) em m/s
    /// </summary>
    public double WeibullC { get; set; }

    /// <summary>
    /// Densidade de potência eólica (W/m²)
    /// </summary>
    public double PowerDensityWM2 { get; set; }

    /// <summary>
    /// Classificação do recurso eólico (IEC 61400 ou NREL 1 a 7)
    /// </summary>
    [MaxLength(80)]
    public string WindClass { get; set; } = string.Empty;

    /// <summary>
    /// Capacidade instalada total do parque em MW
    /// </summary>
    public double TotalCapacityMW { get; set; }

    /// <summary>
    /// Produção Anual de Energia Bruta (Gross AEP) em MWh/ano
    /// </summary>
    public double GrossAepMWh { get; set; }

    /// <summary>
    /// Produção Anual de Energia Líquida (Net AEP) com perdas em MWh/ano
    /// </summary>
    public double NetAepMWh { get; set; }

    /// <summary>
    /// Fator de capacidade líquido do parque (%)
    /// </summary>
    public double CapacityFactorPercent { get; set; }

    /// <summary>
    /// Horas equivalentes a plena carga por ano (horas/ano)
    /// </summary>
    public double FullLoadHours { get; set; }

    /// <summary>
    /// Receita anual estimada (R$)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AnnualRevenue { get; set; }

    /// <summary>
    /// Investimento inicial total (CAPEX em R$)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCapex { get; set; }

    /// <summary>
    /// Custo anual de operação e manutenção (OPEX em R$/ano)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AnnualOpex { get; set; }

    /// <summary>
    /// Custo Nivelado da Energia (LCOE em R$/MWh)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LcoePerMWh { get; set; }

    /// <summary>
    /// Payback simples (anos)
    /// </summary>
    public double SimplePaybackYears { get; set; }

    /// <summary>
    /// Valor Presente Líquido (VPL / NPV em R$)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Npv { get; set; }

    /// <summary>
    /// Taxa Interna de Retorno (TIR / IRR em %)
    /// </summary>
    public double IrrPercent { get; set; }

    /// <summary>
    /// Emissões de CO2 evitadas anualmente (toneladas de CO2/ano)
    /// </summary>
    public double Co2AvoidedTonsPerYear { get; set; }

    /// <summary>
    /// Estimativa de residências atendidas pela geração anual
    /// </summary>
    public int HomesPoweredEstimate { get; set; }

    /// <summary>
    /// Distribuição por classes de vento / bins serializada em JSON
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string WindBinsJson { get; set; } = "[]";

    /// <summary>
    /// Estimativa de geração mensal serializada em JSON
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string MonthlyProductionJson { get; set; } = "[]";
}
