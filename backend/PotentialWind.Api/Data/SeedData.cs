using System.Text.Json;
using PotentialWind.Api.Models;
using PotentialWind.Api.Models.DTOs;

namespace PotentialWind.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Garante que o banco de dados e as tabelas estejam criados
        await context.Database.EnsureCreatedAsync();

        if (!context.WindTurbines.Any())
        {
            var turbines = new List<WindTurbine>
            {
                new WindTurbine
                {
                    Manufacturer = "Vestas",
                    ModelName = "V162-6.2 MW EnVentus",
                    RatedPowerKW = 6200,
                    RotorDiameterM = 162,
                    HubHeightM = 125,
                    CutInSpeed = 3.0,
                    RatedSpeed = 11.5,
                    CutOutSpeed = 25.0,
                    PowerCurveJson = GenerateStandardPowerCurve(3.0, 11.5, 25.0, 6200)
                },
                new WindTurbine
                {
                    Manufacturer = "Siemens Gamesa",
                    ModelName = "SG 5.0-145",
                    RatedPowerKW = 5000,
                    RotorDiameterM = 145,
                    HubHeightM = 120,
                    CutInSpeed = 3.0,
                    RatedSpeed = 11.0,
                    CutOutSpeed = 25.0,
                    PowerCurveJson = GenerateStandardPowerCurve(3.0, 11.0, 25.0, 5000)
                },
                new WindTurbine
                {
                    Manufacturer = "GE Vernova",
                    ModelName = "Cypress 5.3-158",
                    RatedPowerKW = 5300,
                    RotorDiameterM = 158,
                    HubHeightM = 120,
                    CutInSpeed = 3.0,
                    RatedSpeed = 11.2,
                    CutOutSpeed = 25.0,
                    PowerCurveJson = GenerateStandardPowerCurve(3.0, 11.2, 25.0, 5300)
                },
                new WindTurbine
                {
                    Manufacturer = "Nordex",
                    ModelName = "N163/5.X Delta4000",
                    RatedPowerKW = 5700,
                    RotorDiameterM = 163,
                    HubHeightM = 118,
                    CutInSpeed = 3.0,
                    RatedSpeed = 11.8,
                    CutOutSpeed = 25.0,
                    PowerCurveJson = GenerateStandardPowerCurve(3.0, 11.8, 25.0, 5700)
                },
                new WindTurbine
                {
                    Manufacturer = "Goldwind",
                    ModelName = "GW155-4.5 MW PMDD",
                    RatedPowerKW = 4500,
                    RotorDiameterM = 155,
                    HubHeightM = 110,
                    CutInSpeed = 2.5,
                    RatedSpeed = 10.5,
                    CutOutSpeed = 22.0,
                    PowerCurveJson = GenerateStandardPowerCurve(2.5, 10.5, 22.0, 4500)
                }
            };

            await context.WindTurbines.AddRangeAsync(turbines);
            await context.SaveChangesAsync();
        }

        if (!context.WindFarms.Any())
        {
            var defaultTurbine = context.WindTurbines.First();

            var farm1 = new WindFarm
            {
                Name = "Complexo Eólico Ventos da Bahia",
                Description = "Estudo de viabilidade de parque eólico localizado no semiárido baiano, com ventos alísios constantes e alto fator de capacidade.",
                Location = "Caetité - Bahia",
                Latitude = -14.07,
                Longitude = -42.48,
                AltitudeM = 890.0,
                RoughnessExponent = 0.14,
                RoughnessLengthZ0 = 0.03,
                AirDensityKgM3 = 1.15, // Ajustado para altitude de 890m
                SelectedTurbineId = defaultTurbine.Id,
                TurbineCount = 20,
                TurbineSpacingD = 6.0,
                WakeLossPercentage = 6.8,
                ElectricalLossPercentage = 2.2,
                AvailabilityLossPercentage = 2.5,
                OtherLossesPercentage = 1.8,
                EnergyPriceMWh = 210.00m,
                CapexPerMW = 5200000.00m,
                OpexAnnualPercentCapex = 2.2,
                DiscountRatePercent = 8.5,
                ProjectLifespanYears = 25,
                CreatedAt = DateTime.UtcNow
            };

            var farm2 = new WindFarm
            {
                Name = "Parque Eólico Litoral Potiguar",
                Description = "Projeto na costa do Rio Grande do Norte, excelente recurso eólico com ventos oceânicos de alta densidade energética.",
                Location = "São Miguel do Gostoso - RN",
                Latitude = -5.12,
                Longitude = -35.68,
                AltitudeM = 25.0,
                RoughnessExponent = 0.11, // Baixa rugosidade (costeiro)
                RoughnessLengthZ0 = 0.005,
                AirDensityKgM3 = 1.215,
                SelectedTurbineId = defaultTurbine.Id,
                TurbineCount = 16,
                TurbineSpacingD = 5.5,
                WakeLossPercentage = 5.9,
                ElectricalLossPercentage = 2.0,
                AvailabilityLossPercentage = 2.5,
                OtherLossesPercentage = 1.5,
                EnergyPriceMWh = 225.00m,
                CapexPerMW = 5400000.00m,
                OpexAnnualPercentCapex = 2.4,
                DiscountRatePercent = 8.0,
                ProjectLifespanYears = 25,
                CreatedAt = DateTime.UtcNow
            };

            await context.WindFarms.AddRangeAsync(farm1, farm2);
            await context.SaveChangesAsync();

            // Adiciona medições de exemplo para o Parque 1
            var random = new Random(42);
            var measurements = new List<WindMeasurement>();
            var startDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 0; i < 720; i++) // 30 dias de dados horários
            {
                var time = startDate.AddHours(i);
                // Ciclo diurno com média de ~8.8 m/s e variação
                var hourOfDay = time.Hour;
                var diurnalVariation = 1.5 * Math.Sin((hourOfDay - 6) * Math.PI / 12.0);
                var speed = Math.Max(0.5, 8.5 + diurnalVariation + (random.NextDouble() * 3.5 - 1.75));
                var direction = (110.0 + (random.NextDouble() * 40 - 20) + 360) % 360; // Ventos de Sudeste/Leste

                measurements.Add(new WindMeasurement
                {
                    WindFarmId = farm1.Id,
                    Timestamp = time,
                    WindSpeed = Math.Round(speed, 2),
                    WindDirectionDeg = Math.Round(direction, 1),
                    MeasurementHeightM = 50.0,
                    TemperatureC = Math.Round(23.0 + random.NextDouble() * 6.0, 1),
                    PressureHPa = Math.Round(915.0 + random.NextDouble() * 8.0, 1)
                });
            }

            await context.WindMeasurements.AddRangeAsync(measurements);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gera a curva de potência cúbica / sigmoidal típica de um aerogerador moderno
    /// </summary>
    private static string GenerateStandardPowerCurve(double cutIn, double rated, double cutOut, double ratedPowerKW)
    {
        var points = new List<PowerCurvePointDto>();

        for (double v = 0; v <= 30.0; v += 0.5)
        {
            double p = 0;
            if (v < cutIn || v > cutOut)
            {
                p = 0;
            }
            else if (v >= rated)
            {
                p = ratedPowerKW;
            }
            else
            {
                // Região 2 da curva de potência (proporcional a v³ até atingir a velocidade nominal)
                double fraction = (v - cutIn) / (rated - cutIn);
                p = ratedPowerKW * Math.Pow(fraction, 2.5);
                if (p > ratedPowerKW) p = ratedPowerKW;
            }

            points.Add(new PowerCurvePointDto
            {
                WindSpeed = Math.Round(v, 1),
                PowerKW = Math.Round(p, 1)
            });
        }

        return JsonSerializer.Serialize(points);
    }
}
