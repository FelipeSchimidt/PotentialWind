namespace PotentialWind.Api.Services;

public static class WakeLossCalculator
{
    /// <summary>
    /// Estima a perda percentual por efeito esteira (Wake Loss %) em um layout de parque eólico
    /// baseado na distância relativa entre turbinas (em diâmetros de rotor D) e número de turbinas
    /// utilizando aproximação do modelo linear de Jensen (Park Model).
    /// </summary>
    /// <param name="turbineSpacingD">Espaçamento entre turbinas em diâmetros de rotor D (ex: 5.0 a 8.0 D)</param>
    /// <param name="turbineCount">Número total de aerogeradores</param>
    /// <param name="isOffshore">Se o parque for offshore (menor rugosidade e menor decaimento de esteira)</param>
    /// <returns>Perda percentual estimada (%)</returns>
    public static double EstimateWakeLossPercentage(double turbineSpacingD, int turbineCount, bool isOffshore = false)
    {
        if (turbineCount <= 1) return 0.0;
        if (turbineSpacingD < 3.0) turbineSpacingD = 3.0;

        // Coeficiente de decaimento de esteira (Wake decay constant - Jensen)
        // kw onshore ~ 0.075; kw offshore ~ 0.04
        double kw = isOffshore ? 0.045 : 0.075;

        // Coeficiente de empuxo típico de turbina eólica moderna em regime ótimo (Ct ~ 0.8)
        double ct = 0.80;

        // Déficit de velocidade na primeira fileira a jusante
        // deltaV / V0 = (1 - sqrt(1 - Ct)) / (1 + 2 * kw * (spacingD))^2
        double denom = Math.Pow(1.0 + 2.0 * kw * turbineSpacingD, 2.0);
        double singleWakeDeficit = (1.0 - Math.Sqrt(Math.Max(0.01, 1.0 - ct))) / denom;

        // Em um parque eólico de N turbinas em malha (rows x cols):
        // A primeira fileira tem 0% de perda de esteira, e as fileiras subsequentes acumulam déficit
        int rows = Math.Max(2, (int)Math.Ceiling(Math.Sqrt(turbineCount)));
        double averageDeficit = singleWakeDeficit * ((double)(rows - 1) / rows) * 0.70;

        // A perda de potência do parque aproxima-se de 3 * averageDeficit para pequenos déficits
        // ou 1 - (1 - averageDeficit)^3
        double lossFraction = 1.0 - Math.Pow(1.0 - averageDeficit, 3.0);

        // Converte para percentual e limita a perdas realistas da indústria eólica (3% a 18%)
        double lossPercentage = lossFraction * 100.0;
        if (lossPercentage < 3.0) lossPercentage = 3.0;
        if (lossPercentage > 18.0) lossPercentage = 18.0;

        return Math.Round(lossPercentage, 2);
    }

    /// <summary>
    /// Estima a área superficial requerida para o parque eólico (km² e hectares)
    /// com base na regra padrão de espaçamento (5D a jusante x 3D transversal)
    /// </summary>
    public static (double AreaKm2, double AreaHectares) EstimateRequiredLandArea(int turbineCount, double rotorDiameterM, double spacingD = 5.0)
    {
        if (turbineCount <= 0 || rotorDiameterM <= 0) return (0, 0);

        // Espaçamento longitudinal (5D) e transversal (3D)
        double downwindSpacing = spacingD * rotorDiameterM;
        double crosswindSpacing = Math.Max(3.0, spacingD * 0.6) * rotorDiameterM;

        // Área por turbina em metros quadrados
        double areaPerTurbineM2 = downwindSpacing * crosswindSpacing;
        double totalAreaM2 = areaPerTurbineM2 * turbineCount;

        double areaKm2 = totalAreaM2 / 1_000_000.0;
        double areaHectares = totalAreaM2 / 10_000.0;

        return (Math.Round(areaKm2, 2), Math.Round(areaHectares, 1));
    }
}
