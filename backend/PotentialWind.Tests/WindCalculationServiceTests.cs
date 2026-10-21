using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;
using PotentialWind.Api.Services;
using Xunit;

namespace PotentialWind.Tests;

public class WindCalculationServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        SeedData.InitializeAsync(context).GetAwaiter().GetResult();
        return context;
    }

    [Fact]
    public async Task AnalyzeQuickPotentialAsync_ValidRequest_ShouldReturnValidResponse()
    {
        using var context = GetInMemoryDbContext();
        var service = new WindCalculationService(context);

        var request = new QuickWeibullAnalysisRequest
        {
            AverageWindSpeed = 8.5,
            ReferenceHeightM = 50.0,
            TargetHeightM = 120.0,
            RoughnessAlpha = 0.14,
            AirDensityKgM3 = 1.225
        };

        var result = await service.AnalyzeQuickPotentialAsync(request);

        Assert.NotNull(result);
        Assert.Equal(50.0, result.ReferenceHeightM);
        Assert.Equal(120.0, result.TargetHeightM);
        Assert.True(result.ExtrapolatedSpeedMs > result.ReferenceSpeedMs);
        Assert.True(result.WeibullK > 0);
        Assert.True(result.WeibullC > 0);
        Assert.True(result.PowerDensityWM2 > 0);
        Assert.NotNull(result.WindClass);
        Assert.NotEmpty(result.WindBins);
        Assert.Equal(50, result.WindBins.Count); // Bins de 0.5 a 25.0 com passo 0.5
    }

    [Fact]
    public async Task AnalyzeQuickPotentialAsync_WithTurbine_ShouldComputeSingleTurbineAep()
    {
        using var context = GetInMemoryDbContext();
        var service = new WindCalculationService(context);
        var turbine = await context.WindTurbines.FirstAsync();

        var request = new QuickWeibullAnalysisRequest
        {
            AverageWindSpeed = 9.0,
            ReferenceHeightM = 50.0,
            TargetHeightM = 120.0,
            TurbineId = turbine.Id
        };

        var result = await service.AnalyzeQuickPotentialAsync(request);

        Assert.NotNull(result.SingleTurbineGrossAepMWh);
        Assert.True(result.SingleTurbineGrossAepMWh > 0);
        Assert.NotNull(result.SingleTurbineCapacityFactorPercent);
        Assert.True(result.SingleTurbineCapacityFactorPercent > 0 && result.SingleTurbineCapacityFactorPercent < 100);
    }

    [Fact]
    public void ComputeMonthlyProduction_ShouldSumToAnnualGrossAndApplyLosses()
    {
        using var context = GetInMemoryDbContext();
        var service = new WindCalculationService(context);

        double annualGrossMWh = 100000.0;
        double lossFactor = 0.15; // 15% de perdas
        decimal price = 200.0m;

        var monthly = service.ComputeMonthlyProduction(annualGrossMWh, lossFactor, price);

        Assert.Equal(12, monthly.Count);
        double totalGross = monthly.Sum(m => m.GrossMWh);
        double totalNet = monthly.Sum(m => m.NetMWh);

        Assert.Equal(annualGrossMWh, Math.Round(totalGross, 1));
        Assert.Equal(annualGrossMWh * (1 - lossFactor), Math.Round(totalNet, 1));
    }

    [Fact]
    public async Task RunWindFarmSimulationAsync_ExistingFarm_ShouldSimulateAndPersistResult()
    {
        using var context = GetInMemoryDbContext();
        var service = new WindCalculationService(context);
        var farm = await context.WindFarms.FirstAsync();

        var request = new WindFarmSimulationRequest
        {
            WindFarmId = farm.Id,
            TurbineCount = 15
        };

        var result = await service.RunWindFarmSimulationAsync(request);

        Assert.NotNull(result);
        Assert.Equal(farm.Id, result.WindFarmId);
        Assert.True(result.GrossAepMWh > result.NetAepMWh);
        Assert.True(result.CapacityFactorPercent > 0);
        Assert.True(result.TotalCapex > 0);
        Assert.True(result.Npv != 0);

        // Verifica persistência no DB
        var persisted = await context.WindSimulationResults.FindAsync(result.Id);
        Assert.NotNull(persisted);
    }
}
