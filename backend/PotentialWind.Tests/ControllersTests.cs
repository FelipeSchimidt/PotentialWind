using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Controllers;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;
using PotentialWind.Api.Services;
using Xunit;

namespace PotentialWind.Tests;

public class ControllersTests
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
    public async Task WindTurbinesController_GetTurbines_ShouldReturnListOfTurbines()
    {
        using var context = GetInMemoryDbContext();
        var controller = new WindTurbinesController(context);

        var actionResult = await controller.GetTurbines();
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var turbines = Assert.IsAssignableFrom<IEnumerable<WindTurbine>>(okResult.Value);

        Assert.NotEmpty(turbines);
    }

    [Fact]
    public async Task WindTurbinesController_GetTurbine_ExistingId_ShouldReturnTurbine()
    {
        using var context = GetInMemoryDbContext();
        var controller = new WindTurbinesController(context);
        var firstTurbine = await context.WindTurbines.FirstAsync();

        var actionResult = await controller.GetTurbine(firstTurbine.Id);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var turbine = Assert.IsType<WindTurbine>(okResult.Value);

        Assert.Equal(firstTurbine.Id, turbine.Id);
    }

    [Fact]
    public async Task WindTurbinesController_GetTurbine_NonExistingId_ShouldReturnNotFound()
    {
        using var context = GetInMemoryDbContext();
        var controller = new WindTurbinesController(context);

        var actionResult = await controller.GetTurbine(9999);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task WindCalculationsController_QuickAnalysis_ShouldReturnOk()
    {
        using var context = GetInMemoryDbContext();
        var service = new WindCalculationService(context);
        var controller = new WindCalculationsController(service);

        var request = new QuickWeibullAnalysisRequest
        {
            AverageWindSpeed = 8.0,
            ReferenceHeightM = 50.0,
            TargetHeightM = 120.0
        };

        var actionResult = await controller.QuickAnalysis(request);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<QuickWeibullAnalysisResponse>(okResult.Value);

        Assert.NotNull(response);
        Assert.True(response.ExtrapolatedSpeedMs > 8.0);
    }
}
