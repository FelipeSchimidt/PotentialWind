using PotentialWind.Api.Services;
using Xunit;

namespace PotentialWind.Tests;

public class WeibullCalculatorTests
{
    [Fact]
    public void GammaFunction_ShouldMatchKnownValues()
    {
        // Gamma(1) = 0! = 1
        Assert.Equal(1.0, WeibullCalculator.Gamma(1.0), precision: 4);

        // Gamma(2) = 1! = 1
        Assert.Equal(1.0, WeibullCalculator.Gamma(2.0), precision: 4);

        // Gamma(3) = 2! = 2
        Assert.Equal(2.0, WeibullCalculator.Gamma(3.0), precision: 4);

        // Gamma(4) = 3! = 6
        Assert.Equal(6.0, WeibullCalculator.Gamma(4.0), precision: 4);

        // Gamma(0.5) = sqrt(pi) ~ 1.77245
        Assert.Equal(Math.Sqrt(Math.PI), WeibullCalculator.Gamma(0.5), precision: 4);
    }

    [Fact]
    public void EstimateFromMeanAndStdDev_ShouldProduceReasonableParameters()
    {
        double mean = 8.5;
        double stdDev = 3.8;

        var (k, c) = WeibullCalculator.EstimateFromMeanAndStdDev(mean, stdDev);

        Assert.True(k > 1.5 && k < 3.0, $"Expected k between 1.5 and 3.0, got {k}");
        Assert.True(c > mean, $"Expected scale factor c > mean, got c={c}, mean={mean}");
    }

    [Fact]
    public void PowerDensity_ShouldIncreaseWithScaleFactor()
    {
        double k = 2.0;
        double cLow = 7.0;
        double cHigh = 10.0;

        double pdLow = WeibullCalculator.CalculatePowerDensity(k, cLow);
        double pdHigh = WeibullCalculator.CalculatePowerDensity(k, cHigh);

        Assert.True(pdHigh > pdLow * 2.0, "Power density should increase with c^3");
    }

    [Fact]
    public void ExtrapolateHeightPowerLaw_HigherHub_ShouldYieldHigherSpeed()
    {
        double speed50m = 8.0;
        double hubHeight = 120.0;
        double alpha = 0.14;

        double speedHub = WeibullCalculator.ExtrapolateHeightPowerLaw(speed50m, 50.0, hubHeight, alpha);

        Assert.True(speedHub > speed50m, "Wind speed at hub height must be greater than at 50m");
        Assert.Equal(9.04, Math.Round(speedHub, 2));
    }

    [Fact]
    public void ClassifyWindResource_ShouldReturnCorrectIECClasses()
    {
        var lowClass = WeibullCalculator.ClassifyWindResource(180, 5.5);
        Assert.Contains("Classe 1", lowClass.WindClass);

        var goodClass = WeibullCalculator.ClassifyWindResource(380, 7.5);
        Assert.Contains("Classe 3", goodClass.WindClass);

        var excellentClass = WeibullCalculator.ClassifyWindResource(550, 9.0);
        Assert.Contains("Classe 5", excellentClass.WindClass);
    }
}
