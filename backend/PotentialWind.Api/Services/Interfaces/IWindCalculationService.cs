using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;

namespace PotentialWind.Api.Services.Interfaces;

public interface IWindCalculationService
{
    Task<QuickWeibullAnalysisResponse> AnalyzeQuickPotentialAsync(QuickWeibullAnalysisRequest request);
    Task<WindSimulationResult> RunWindFarmSimulationAsync(WindFarmSimulationRequest request);
    List<WindBinDto> ComputeWindDistributionBins(double k, double c, List<PowerCurvePointDto>? powerCurve);
    List<MonthlyProductionDto> ComputeMonthlyProduction(double annualGrossMWh, double totalLossFactor, decimal energyPriceMWh);
}
