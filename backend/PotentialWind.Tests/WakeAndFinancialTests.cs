using PotentialWind.Api.Services;
using Xunit;

namespace PotentialWind.Tests;

public class WakeAndFinancialTests
{
    [Fact]
    public void WakeLoss_CloserSpacing_ShouldProduceHigherLosses()
    {
        int turbineCount = 20;
        double tightSpacingLoss = WakeLossCalculator.EstimateWakeLossPercentage(4.0, turbineCount);
        double wideSpacingLoss = WakeLossCalculator.EstimateWakeLossPercentage(8.0, turbineCount);

        Assert.True(tightSpacingLoss > wideSpacingLoss, "Tighter spacing must generate higher wake losses");
    }

    [Fact]
    public void LandFootprint_ShouldScaleWithTurbineCountAndRotor()
    {
        int turbineCount = 10;
        double rotorDiameterM = 150.0;
        double spacingD = 5.0;

        var (areaKm2, areaHectares) = WakeLossCalculator.EstimateRequiredLandArea(turbineCount, rotorDiameterM, spacingD);

        Assert.True(areaKm2 > 0);
        Assert.True(areaHectares > 0);
        Assert.True(Math.Abs(areaKm2 * 100.0 - areaHectares) < 1.0);
    }

    [Fact]
    public void FinancialAnalysis_ViableProject_ShouldReturnPositiveNpvAndReasonablePayback()
    {
        double totalCapacityMW = 100.0;
        double netAepMWh = 420000.0; // ~48% capacity factor
        decimal capexPerMW = 5000000m; // R$ 5M / MW -> R$ 500M total
        double opexPercent = 2.5; // 2.5% ao ano -> R$ 12.5M/ano
        decimal energyPriceMWh = 220.00m; // R$ 220 / MWh -> R$ 92.4M/ano receita
        double discountRate = 8.5;

        var result = FinancialCalculator.Calculate(
            totalCapacityMW,
            netAepMWh,
            capexPerMW,
            opexPercent,
            energyPriceMWh,
            discountRate,
            lifespanYears: 25);

        Assert.Equal(500000000m, result.TotalCapex);
        Assert.True(result.AnnualRevenue > 90000000m);
        Assert.True(result.SimplePaybackYears > 4.0 && result.SimplePaybackYears < 10.0);
        Assert.True(result.Npv > 0, "Viable wind project with 48% CF should yield positive NPV");
        Assert.True(result.IrrPercent > discountRate, "IRR should exceed discount rate for profitable project");
        Assert.True(result.LcoePerMWh > 80m && result.LcoePerMWh < 180m, $"Expected LCOE between 80 and 180, got {result.LcoePerMWh}");
        Assert.True(result.HomesPoweredEstimate > 100000);
    }
}
