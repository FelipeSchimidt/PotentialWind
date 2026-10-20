namespace PotentialWind.Api.Services;

public static class WeibullCalculator
{
    /// <summary>
    /// Aproximação de Lanczos para a função Gamma de Euler Gamma(z)
    /// </summary>
    public static double Gamma(double z)
    {
        if (z < 0.5)
        {
            // Fórmula de reflexão de Euler
            return Math.PI / (Math.Sin(Math.PI * z) * Gamma(1.0 - z));
        }

        z -= 1.0;
        double[] p = {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.138571095836526,
            9.9843695780195716e-6,
            1.5056327351493116e-7
        };

        double x = p[0];
        for (int i = 1; i < p.Length; i++)
        {
            x += p[i] / (z + i);
        }

        double t = z + 7.5;
        return Math.Sqrt(2.0 * Math.PI) * Math.Pow(t, z + 0.5) * Math.Exp(-t) * x;
    }

    /// <summary>
    /// Estima os parâmetros k (forma) e c (escala) de Weibull a partir da velocidade média e desvio padrão
    /// Utiliza o método empírico padronizado de Justus
    /// </summary>
    public static (double k, double c) EstimateFromMeanAndStdDev(double meanSpeed, double stdDev)
    {
        if (meanSpeed <= 0) meanSpeed = 1.0;
        if (stdDev <= 0) stdDev = meanSpeed * 0.45; // Desvio padrão padrão típico se não fornecido

        double ratio = stdDev / meanSpeed;
        double k = Math.Pow(ratio, -1.086);
        if (k < 1.0) k = 1.0;
        if (k > 5.0) k = 5.0;

        double c = meanSpeed / Gamma(1.0 + (1.0 / k));
        return (k, c);
    }

    /// <summary>
    /// Estima c a partir de k e da velocidade média conhecida
    /// </summary>
    public static double ScaleFromKAndMean(double k, double meanSpeed)
    {
        if (k <= 0) k = 2.0;
        return meanSpeed / Gamma(1.0 + (1.0 / k));
    }

    /// <summary>
    /// Função densidade de probabilidade (PDF) de Weibull para a velocidade v:
    /// f(v) = (k/c) * (v/c)^(k-1) * exp(-(v/c)^k)
    /// </summary>
    public static double ProbabilityDensity(double v, double k, double c)
    {
        if (v < 0 || c <= 0 || k <= 0) return 0.0;
        if (v == 0) return k == 1.0 ? 1.0 / c : 0.0;

        double fraction = v / c;
        return (k / c) * Math.Pow(fraction, k - 1.0) * Math.Exp(-Math.Pow(fraction, k));
    }

    /// <summary>
    /// Função de distribuição cumulativa (CDF) de Weibull:
    /// F(v) = 1 - exp(-(v/c)^k)
    /// </summary>
    public static double CumulativeProbability(double v, double k, double c)
    {
        if (v <= 0 || c <= 0 || k <= 0) return 0.0;
        return 1.0 - Math.Exp(-Math.Pow(v / c, k));
    }

    /// <summary>
    /// Calcula a Densidade de Potência Eólica teórica (W/m²):
    /// P_d = 0.5 * rho * c³ * Gamma(1 + 3/k)
    /// </summary>
    public static double CalculatePowerDensity(double k, double c, double airDensity = 1.225)
    {
        if (k <= 0 || c <= 0) return 0.0;
        double gammaVal = Gamma(1.0 + (3.0 / k));
        return 0.5 * airDensity * Math.Pow(c, 3.0) * gammaVal;
    }

    /// <summary>
    /// Extrapola a velocidade do vento da altura de medição para a altura do cubo (Hub Height)
    /// usando a Lei de Potência de Hellmann: v(h2) = v(h1) * (h2 / h1)^alpha
    /// </summary>
    public static double ExtrapolateHeightPowerLaw(double speedRef, double hRef, double hTarget, double alpha = 0.14)
    {
        if (hRef <= 0 || hTarget <= 0 || speedRef <= 0) return speedRef;
        return speedRef * Math.Pow(hTarget / hRef, alpha);
    }

    /// <summary>
    /// Extrapolação pela Lei Logarítmica: v(h2) = v(h1) * ln(h2 / z0) / ln(h1 / z0)
    /// </summary>
    public static double ExtrapolateHeightLogLaw(double speedRef, double hRef, double hTarget, double z0 = 0.03)
    {
        if (hRef <= z0 || hTarget <= z0 || speedRef <= 0) return speedRef;
        return speedRef * (Math.Log(hTarget / z0) / Math.Log(hRef / z0));
    }

    /// <summary>
    /// Categoriza a densidade eólica de acordo com as classes NREL / IEC
    /// </summary>
    public static (string WindClass, string Recommendation) ClassifyWindResource(double powerDensityWM2, double speedAtHub)
    {
        if (powerDensityWM2 < 200 || speedAtHub < 6.0)
        {
            return ("Classe 1 - Recurso Marginal", "Inviável comercialmente para parques de grande porte; viável apenas para microgeração distribuída.");
        }
        if (powerDensityWM2 < 300 || speedAtHub < 7.0)
        {
            return ("Classe 2 - Moderado", "Potencial aceitável; requer turbinas de grande área de varredura (alto rotor para baixo vento - IEC Class III).");
        }
        if (powerDensityWM2 < 400 || speedAtHub < 7.8)
        {
            return ("Classe 3 - Bom", "Bom potencial comercial para parques eólicos com excelente viabilidade com aerogeradores classe IEC II/III.");
        }
        if (powerDensityWM2 < 500 || speedAtHub < 8.6)
        {
            return ("Classe 4 - Muito Bom", "Excelente viabilidade técnica e financeira; perfil típico dos melhores sítios eólicos no Nordeste brasileiro.");
        }
        if (powerDensityWM2 < 600 || speedAtHub < 9.4)
        {
            return ("Classe 5 - Excelente", "Sítio de classe mundial; fatores de capacidade frequentemente superiores a 50%.");
        }
        if (powerDensityWM2 < 800)
        {
            return ("Classe 6 - Excepcional", "Altíssimo potencial energético; projeto com rápida amortização do capital (payback curto).");
        }

        return ("Classe 7 - Extraordinário", "Potencial energético extraordinário; sítio de altíssimo rendimento.");
    }
}
