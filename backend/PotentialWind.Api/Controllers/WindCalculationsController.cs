using Microsoft.AspNetCore.Mvc;
using PotentialWind.Api.Models.DTOs;
using PotentialWind.Api.Services;
using PotentialWind.Api.Services.Interfaces;

namespace PotentialWind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WindCalculationsController : ControllerBase
{
    private readonly IWindCalculationService _calculationService;

    public WindCalculationsController(IWindCalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    /// <summary>
    /// Executa análise rápida de potencial de Weibull, densidade de potência e extrapolação de altura
    /// </summary>
    [HttpPost("quick-analysis")]
    public async Task<ActionResult<QuickWeibullAnalysisResponse>> QuickAnalysis([FromBody] QuickWeibullAnalysisRequest request)
    {
        try
        {
            var response = await _calculationService.AnalyzeQuickPotentialAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Executa simulação completa do parque eólico (AEP Bruto/Líquido, perdas, Fator de Capacidade e LCOE/VPL/TIR)
    /// </summary>
    [HttpPost("simulate-farm")]
    public async Task<IActionResult> SimulateWindFarm([FromBody] WindFarmSimulationRequest request)
    {
        try
        {
            var result = await _calculationService.RunWindFarmSimulationAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Calcula perdas estimadas por efeito esteira (Jensen Park Model) para dada distância e número de turbinas
    /// </summary>
    [HttpGet("wake-loss")]
    public IActionResult EstimateWakeLoss([FromQuery] double spacingD, [FromQuery] int turbineCount, [FromQuery] bool isOffshore = false)
    {
        double lossPercent = WakeLossCalculator.EstimateWakeLossPercentage(spacingD, turbineCount, isOffshore);
        return Ok(new { spacingD, turbineCount, isOffshore, wakeLossPercent = lossPercent });
    }

    /// <summary>
    /// Extrapolação vertical da velocidade do vento entre duas alturas
    /// </summary>
    [HttpGet("shear-extrapolation")]
    public IActionResult ExtrapolateShear(
        [FromQuery] double speedRef,
        [FromQuery] double heightRef,
        [FromQuery] double heightTarget,
        [FromQuery] double roughnessAlpha = 0.14)
    {
        double speedTarget = WeibullCalculator.ExtrapolateHeightPowerLaw(speedRef, heightRef, heightTarget, roughnessAlpha);
        return Ok(new
        {
            referenceSpeed = speedRef,
            referenceHeight = heightRef,
            targetHeight = heightTarget,
            roughnessAlpha,
            extrapolatedSpeed = Math.Round(speedTarget, 2)
        });
    }
}
