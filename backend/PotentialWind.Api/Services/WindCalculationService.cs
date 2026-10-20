using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;
using PotentialWind.Api.Services.Interfaces;

namespace PotentialWind.Api.Services;

public class WindCalculationService : IWindCalculationService
{
    private readonly AppDbContext _context;

    public WindCalculationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QuickWeibullAnalysisResponse> AnalyzeQuickPotentialAsync(QuickWeibullAnalysisRequest request)
    {
        // 1. Extrapolação de altura
        double extrapolatedSpeed = WeibullCalculator.ExtrapolateHeightPowerLaw(
            request.AverageWindSpeed,
            request.ReferenceHeightM,
            request.TargetHeightM,
            request.RoughnessAlpha);

        // 2. Determinação de k e c
        double k;
        double c;

        if (request.WeibullK.HasValue && request.WeibullK.Value > 0)
        {
            k = request.WeibullK.Value;
            c = request.WeibullC.HasValue && request.WeibullC.Value > 0
                ? request.WeibullC.Value
                : WeibullCalculator.ScaleFromKAndMean(k, extrapolatedSpeed);
        }
        else if (request.StandardDeviation.HasValue && request.StandardDeviation.Value > 0)
        {
            var estimated = WeibullCalculator.EstimateFromMeanAndStdDev(extrapolatedSpeed, request.StandardDeviation.Value);
            k = estimated.k;
            c = estimated.c;
        }
        else
        {
            // Padrão empírico para sítios eólicos de boa qualidade (k = 2.1)
            k = 2.15;
            c = WeibullCalculator.ScaleFromKAndMean(k, extrapolatedSpeed);
        }

        // 3. Densidade de potência eólica teórica (W/m²)
        double powerDensity = WeibullCalculator.CalculatePowerDensity(k, c, request.AirDensityKgM3);
        var classification = WeibullCalculator.ClassifyWindResource(powerDensity, extrapolatedSpeed);

        // 4. Carrega curva de potência da turbina se informada
        List<PowerCurvePointDto>? powerCurve = null;
        WindTurbine? turbine = null;

        if (request.TurbineId.HasValue)
        {
            turbine = await _context.WindTurbines.FindAsync(request.TurbineId.Value);
            if (turbine != null && !string.IsNullOrWhiteSpace(turbine.PowerCurveJson))
            {
                try
                {
                    powerCurve = JsonSerializer.Deserialize<List<PowerCurvePointDto>>(turbine.PowerCurveJson);
                }
                catch
                {
                    // Fallback
                }
            }
        }

        var bins = ComputeWindDistributionBins(k, c, powerCurve);

        // 5. Se houver turbina, calcula o Gross AEP para uma única unidade
        double? singleGrossAep = null;
        double? singleCapacityFactor = null;

        if (powerCurve != null && turbine != null && turbine.RatedPowerKW > 0)
        {
            singleGrossAep = bins.Sum(b => b.EnergyMWh);
            double theoreticalMaxEnergy = (turbine.RatedPowerKW / 1000.0) * 8760.0;
            singleCapacityFactor = (singleGrossAep.Value / theoreticalMaxEnergy) * 100.0;
        }

        return new QuickWeibullAnalysisResponse
        {
            ReferenceHeightM = request.ReferenceHeightM,
            TargetHeightM = request.TargetHeightM,
            ReferenceSpeedMs = Math.Round(request.AverageWindSpeed, 2),
            ExtrapolatedSpeedMs = Math.Round(extrapolatedSpeed, 2),
            WeibullK = Math.Round(k, 3),
            WeibullC = Math.Round(c, 3),
            PowerDensityWM2 = Math.Round(powerDensity, 1),
            WindClass = classification.WindClass,
            Recommendation = classification.Recommendation,
            WindBins = bins,
            SingleTurbineGrossAepMWh = singleGrossAep.HasValue ? Math.Round(singleGrossAep.Value, 1) : null,
            SingleTurbineCapacityFactorPercent = singleCapacityFactor.HasValue ? Math.Round(singleCapacityFactor.Value, 2) : null
        };
    }

    public async Task<WindSimulationResult> RunWindFarmSimulationAsync(WindFarmSimulationRequest request)
    {
        var farm = await _context.WindFarms
            .Include(f => f.SelectedTurbine)
            .Include(f => f.Measurements)
            .FirstOrDefaultAsync(f => f.Id == request.WindFarmId)
            ?? throw new KeyNotFoundException($"Parque eólico com ID {request.WindFarmId} não foi encontrado.");

        int turbineId = request.TurbineId ?? farm.SelectedTurbineId ?? 0;
        var turbine = await _context.WindTurbines.FindAsync(turbineId)
            ?? throw new InvalidOperationException("Nenhum aerogerador foi selecionado ou encontrado para o parque.");

        int turbineCount = request.TurbineCount ?? farm.TurbineCount;
        if (turbineCount <= 0) turbineCount = 10;

        // 1. Determina a velocidade média de referência
        double meanSpeed;
        double stdDev = 0;

        if (request.OverrideAverageWindSpeed.HasValue && request.OverrideAverageWindSpeed.Value > 0)
        {
            meanSpeed = request.OverrideAverageWindSpeed.Value;
        }
        else if (farm.Measurements.Any())
        {
            meanSpeed = farm.Measurements.Average(m => m.WindSpeed);
            double sumSquares = farm.Measurements.Sum(m => Math.Pow(m.WindSpeed - meanSpeed, 2));
            stdDev = Math.Sqrt(sumSquares / farm.Measurements.Count);
        }
        else
        {
            meanSpeed = 8.5; // Valor padrão de sítio eólico viável
        }

        // 2. Altura de referência (da medição ou 50m) e altura do cubo (Hub Height da turbina)
        double refHeight = farm.Measurements.FirstOrDefault()?.MeasurementHeightM ?? 50.0;
        double hubHeight = turbine.HubHeightM > 0 ? turbine.HubHeightM : 120.0;

        // 3. Extrapola velocidade para o cubo do rotor
        double hubSpeed = WeibullCalculator.ExtrapolateHeightPowerLaw(meanSpeed, refHeight, hubHeight, farm.RoughnessExponent);

        // 4. Parâmetros Weibull
        double k;
        double c;

        if (request.OverrideWeibullK.HasValue && request.OverrideWeibullK.Value > 0)
        {
            k = request.OverrideWeibullK.Value;
            c = WeibullCalculator.ScaleFromKAndMean(k, hubSpeed);
        }
        else if (stdDev > 0)
        {
            var estimated = WeibullCalculator.EstimateFromMeanAndStdDev(hubSpeed, stdDev * (hubSpeed / meanSpeed));
            k = estimated.k;
            c = estimated.c;
        }
        else
        {
            k = 2.2;
            c = WeibullCalculator.ScaleFromKAndMean(k, hubSpeed);
        }

        // 5. Densidade de potência teórica do vento
        double powerDensity = WeibullCalculator.CalculatePowerDensity(k, c, farm.AirDensityKgM3);
        var classification = WeibullCalculator.ClassifyWindResource(powerDensity, hubSpeed);

        // 6. Curva de potência da turbina
        List<PowerCurvePointDto> powerCurve = new();
        if (!string.IsNullOrWhiteSpace(turbine.PowerCurveJson))
        {
            try
            {
                powerCurve = JsonSerializer.Deserialize<List<PowerCurvePointDto>>(turbine.PowerCurveJson) ?? new();
            }
            catch
            {
                // Ignora erro de desserialização
            }
        }

        // 7. Bins de velocidade e Gross AEP unitário
        var bins = ComputeWindDistributionBins(k, c, powerCurve);
        double singleTurbineGrossAep = bins.Sum(b => b.EnergyMWh);

        // 8. Capacidade total instalada do parque
        double turbineRatedMW = turbine.RatedPowerKW / 1000.0;
        double totalCapacityMW = turbineRatedMW * turbineCount;

        // 9. Gross AEP do parque todo
        double totalGrossAepMWh = singleTurbineGrossAep * turbineCount;

        // 10. Perdas acumuladas
        // Se o usuário não definiu WakeLoss, calcula via Jensen Park Model
        double wakeLossPct = farm.WakeLossPercentage > 0
            ? farm.WakeLossPercentage
            : WakeLossCalculator.EstimateWakeLossPercentage(farm.TurbineSpacingD, turbineCount);

        double wakeFactor = 1.0 - (wakeLossPct / 100.0);
        double elecFactor = 1.0 - (farm.ElectricalLossPercentage / 100.0);
        double availFactor = 1.0 - (farm.AvailabilityLossPercentage / 100.0);
        double otherFactor = 1.0 - (farm.OtherLossesPercentage / 100.0);

        double totalNetFactor = wakeFactor * elecFactor * availFactor * otherFactor;
        double totalLossFactor = 1.0 - totalNetFactor;

        double netAepMWh = totalGrossAepMWh * totalNetFactor;

        // 11. Fator de capacidade e horas equivalentes
        double maxTheoreticalMWh = totalCapacityMW * 8760.0;
        double capacityFactor = maxTheoreticalMWh > 0 ? (netAepMWh / maxTheoreticalMWh) * 100.0 : 0;
        double fullLoadHours = totalCapacityMW > 0 ? netAepMWh / totalCapacityMW : 0;

        // 12. Viabilidade financeira
        var financial = FinancialCalculator.Calculate(
            totalCapacityMW,
            netAepMWh,
            farm.CapexPerMW,
            farm.OpexAnnualPercentCapex,
            farm.EnergyPriceMWh,
            farm.DiscountRatePercent,
            farm.ProjectLifespanYears);

        // 13. Geração mensal estimada
        var monthlyList = ComputeMonthlyProduction(totalGrossAepMWh, totalLossFactor, farm.EnergyPriceMWh);

        var result = new WindSimulationResult
        {
            WindFarmId = farm.Id,
            CalculatedAt = DateTime.UtcNow,
            AverageWindSpeedAtHub = Math.Round(hubSpeed, 2),
            WeibullK = Math.Round(k, 3),
            WeibullC = Math.Round(c, 3),
            PowerDensityWM2 = Math.Round(powerDensity, 1),
            WindClass = classification.WindClass,
            TotalCapacityMW = Math.Round(totalCapacityMW, 2),
            GrossAepMWh = Math.Round(totalGrossAepMWh, 1),
            NetAepMWh = Math.Round(netAepMWh, 1),
            CapacityFactorPercent = Math.Round(capacityFactor, 2),
            FullLoadHours = Math.Round(fullLoadHours, 1),
            TotalCapex = financial.TotalCapex,
            AnnualOpex = financial.AnnualOpex,
            AnnualRevenue = financial.AnnualRevenue,
            LcoePerMWh = financial.LcoePerMWh,
            SimplePaybackYears = financial.SimplePaybackYears,
            Npv = financial.Npv,
            IrrPercent = financial.IrrPercent,
            Co2AvoidedTonsPerYear = financial.Co2AvoidedTonsPerYear,
            HomesPoweredEstimate = financial.HomesPoweredEstimate,
            WindBinsJson = JsonSerializer.Serialize(bins),
            MonthlyProductionJson = JsonSerializer.Serialize(monthlyList)
        };

        _context.WindSimulationResults.Add(result);
        await _context.SaveChangesAsync();

        return result;
    }

    public List<WindBinDto> ComputeWindDistributionBins(double k, double c, List<PowerCurvePointDto>? powerCurve)
    {
        var bins = new List<WindBinDto>();
        double deltaV = 0.5;

        for (double v = 0.5; v <= 25.0; v += deltaV)
        {
            double vLow = v - (deltaV / 2.0);
            double vHigh = v + (deltaV / 2.0);

            // Probabilidade de ocorrência no intervalo [vLow, vHigh] via CDF de Weibull
            double pLow = WeibullCalculator.CumulativeProbability(vLow, k, c);
            double pHigh = WeibullCalculator.CumulativeProbability(vHigh, k, c);
            double probability = Math.Max(0.0, pHigh - pLow);

            double hoursPerYear = probability * 8760.0;

            // Potência gerada pela turbina na velocidade média do bin v
            double powerKW = 0.0;
            if (powerCurve != null && powerCurve.Any())
            {
                powerKW = InterpolatePower(v, powerCurve);
            }

            // Energia em MWh = (Potência em kW / 1000) * Horas no ano
            double energyMWh = (powerKW / 1000.0) * hoursPerYear;

            bins.Add(new WindBinDto
            {
                WindSpeed = Math.Round(v, 1),
                ProbabilityDensity = Math.Round(WeibullCalculator.ProbabilityDensity(v, k, c), 4),
                CumulativeProbability = Math.Round(pHigh, 4),
                HoursPerYear = Math.Round(hoursPerYear, 1),
                TurbinePowerKW = Math.Round(powerKW, 1),
                EnergyMWh = Math.Round(energyMWh, 2)
            });
        }

        return bins;
    }

    public List<MonthlyProductionDto> ComputeMonthlyProduction(double annualGrossMWh, double totalLossFactor, decimal energyPriceMWh)
    {
        // Distribuição sazonal típica de ventos no Brasil (Nordeste: pico no 2º semestre entre julho e novembro)
        double[] seasonalWeights = { 0.065, 0.055, 0.050, 0.055, 0.070, 0.085, 0.110, 0.125, 0.130, 0.115, 0.085, 0.055 };
        string[] monthNames = { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };

        var list = new List<MonthlyProductionDto>();

        for (int i = 0; i < 12; i++)
        {
            double monthlyGross = annualGrossMWh * seasonalWeights[i];
            double monthlyNet = monthlyGross * (1.0 - totalLossFactor);
            decimal revenue = (decimal)monthlyNet * energyPriceMWh;

            list.Add(new MonthlyProductionDto
            {
                Month = i + 1,
                MonthName = monthNames[i],
                AverageSpeed = Math.Round(7.0 + (seasonalWeights[i] - 0.083) * 35.0, 1),
                GrossMWh = Math.Round(monthlyGross, 1),
                NetMWh = Math.Round(monthlyNet, 1),
                Revenue = Math.Round(revenue, 2)
            });
        }

        return list;
    }

    private static double InterpolatePower(double windSpeed, List<PowerCurvePointDto> curve)
    {
        var sorted = curve.OrderBy(p => p.WindSpeed).ToList();
        if (windSpeed <= sorted.First().WindSpeed) return sorted.First().PowerKW;
        if (windSpeed >= sorted.Last().WindSpeed) return sorted.Last().PowerKW;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (windSpeed >= sorted[i].WindSpeed && windSpeed <= sorted[i + 1].WindSpeed)
            {
                double v1 = sorted[i].WindSpeed;
                double v2 = sorted[i + 1].WindSpeed;
                double p1 = sorted[i].PowerKW;
                double p2 = sorted[i + 1].PowerKW;

                if (Math.Abs(v2 - v1) < 1e-6) return p1;
                return p1 + ((windSpeed - v1) / (v2 - v1)) * (p2 - p1);
            }
        }

        return 0;
    }
}
