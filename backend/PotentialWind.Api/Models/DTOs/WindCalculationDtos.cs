namespace PotentialWind.Api.Models.DTOs;

public class PowerCurvePointDto
{
    public double WindSpeed { get; set; }
    public double PowerKW { get; set; }
}

public class WindBinDto
{
    public double WindSpeed { get; set; }
    public double ProbabilityDensity { get; set; }
    public double CumulativeProbability { get; set; }
    public double HoursPerYear { get; set; }
    public double TurbinePowerKW { get; set; }
    public double EnergyMWh { get; set; }
}

public class MonthlyProductionDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public double AverageSpeed { get; set; }
    public double GrossMWh { get; set; }
    public double NetMWh { get; set; }
    public decimal Revenue { get; set; }
}

public class QuickWeibullAnalysisRequest
{
    /// <summary>
    /// Velocidade média do vento observada (m/s)
    /// </summary>
    public double AverageWindSpeed { get; set; } = 8.5;

    /// <summary>
    /// Desvio padrão da velocidade do vento (opcional, para estimar k e c via Justus)
    /// </summary>
    public double? StandardDeviation { get; set; }

    /// <summary>
    /// Fator de forma Weibull (k) - se já conhecido
    /// </summary>
    public double? WeibullK { get; set; }

    /// <summary>
    /// Fator de escala Weibull (c) - se já conhecido
    /// </summary>
    public double? WeibullC { get; set; }

    /// <summary>
    /// Altura de referência onde o vento foi medido (m)
    /// </summary>
    public double ReferenceHeightM { get; set; } = 50.0;

    /// <summary>
    /// Altura alvo para cálculo (ex: altura do cubo do rotor, 120m)
    /// </summary>
    public double TargetHeightM { get; set; } = 120.0;

    /// <summary>
    /// Coeficiente de cisalhamento do terreno (alpha da Lei de Potência)
    /// </summary>
    public double RoughnessAlpha { get; set; } = 0.14;

    /// <summary>
    /// Densidade do ar local (kg/m³)
    /// </summary>
    public double AirDensityKgM3 { get; set; } = 1.225;

    /// <summary>
    /// ID do aerogerador opcional para cálculo de potência e AEP
    /// </summary>
    public int? TurbineId { get; set; }
}

public class QuickWeibullAnalysisResponse
{
    public double ReferenceHeightM { get; set; }
    public double TargetHeightM { get; set; }
    public double ReferenceSpeedMs { get; set; }
    public double ExtrapolatedSpeedMs { get; set; }
    public double WeibullK { get; set; }
    public double WeibullC { get; set; }
    public double PowerDensityWM2 { get; set; }
    public string WindClass { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public List<WindBinDto> WindBins { get; set; } = new();
    public double? SingleTurbineGrossAepMWh { get; set; }
    public double? SingleTurbineCapacityFactorPercent { get; set; }
}

public class WindFarmSimulationRequest
{
    public int WindFarmId { get; set; }
    public int? TurbineId { get; set; }
    public int? TurbineCount { get; set; }
    public double? OverrideAverageWindSpeed { get; set; }
    public double? OverrideWeibullK { get; set; }
}

public class DatabaseStatusDto
{
    public string CurrentProvider { get; set; } = string.Empty;
    public string ConnectionStringSample { get; set; } = string.Empty;
    public bool CanConnect { get; set; }
    public int WindFarmsCount { get; set; }
    public int WindTurbinesCount { get; set; }
    public int MeasurementsCount { get; set; }
    public string MigrationNotes { get; set; } = string.Empty;
}

public class BatchMeasurementsImportDto
{
    public int WindFarmId { get; set; }
    public double MeasurementHeightM { get; set; } = 50.0;
    public List<MeasurementPointDto> Points { get; set; } = new();
}

public class MeasurementPointDto
{
    public DateTime Timestamp { get; set; }
    public double WindSpeed { get; set; }
    public double WindDirectionDeg { get; set; }
    public double? TemperatureC { get; set; }
    public double? PressureHPa { get; set; }
}
